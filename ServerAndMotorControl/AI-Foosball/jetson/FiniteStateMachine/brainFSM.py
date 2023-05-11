from curses.ascii import TAB
import socket
import time
import asyncio
import math
from rodFSM import compute_next_state, compute_command
from Message import Message
from FSMConstants import *

def connect_to_server(host, port):
	while True:	
		try:
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.connect((host, port))
			return sock
		except:
			time.sleep(2)
			continue

def compute_intercepts(goalX, twoX, fiveX, threeX, width, ballX, ballY, ball_vel_x, ball_vel_y):
    intercepts = [-1, -1, -1, -1]
    if ball_vel_x == 0:
        return intercepts
    slope = ball_vel_y / ball_vel_x
    b = ballY - slope * ballX
    if ball_vel_x > 0:
        direction = True
    else:
        direction = False
    
    if (direction and ballX < goalX) or (not direction and ballX > goalX):
        intercept = slope * ballX + b
        if 0 <= intercept <= width:
            intercepts[0] = intercept
    if (direction and ballX < twoX) or (not direction and ballX > twoX):
        intercept = slope * ballX + b
        if 0 <= intercept <= width:
            intercepts[1] = intercept
    if (direction and ballX < fiveX) or (not direction and ballX > fiveX):
        intercept = slope * ballX + b
        if 0 <= intercept <= width:
            intercepts[2] = intercept
    if (direction and ballX < threeX) or (not direction and ballX > threeX):
        intercept = slope * ballX + b
        if 0 <= intercept <= width:
            intercepts[3] = intercept
    
    return intercepts

def ball_speed(ball_vel_x, ball_vel_y):
    return math.sqrt(ball_vel_x**2 + ball_vel_y**2)


async def main():
    current_states = ["Block", "Block", "Block", "Block"]
    kick_timeout = [0, False, 0, False, 0, False, 0, False]

    commands = {"robot_goal_rod_displacement_command":0, "robot_goal_rod_angle_command":0, "robot_2_rod_displacement_command":0, "robot_2_rod_angle_command":0, "robot_5_rod_displacement_command":0, "robot_5_rod_angle_command":0, "robot_3_rod_displacement_command":0, "robot_3_rod_angle_command":0}
    server_data = {"player_score":0, "robot_score":0, "stop":False, "ball_x":0, "ball_y":0, "ball_Vx":0, "ball_Vy":0, "robot_goal_rod_displacement_current":0, "robot_goal_rod_angle_current":0, "robot_2_rod_displacement_current":0, "robot_2_rod_angle_current":0, "robot_5_rod_displacement_current":0, "robot_5_rod_angle_current":0, "robot_3_rod_displacement_current":0, "robot_3_rod_angle_current":0}

    try:
        host = PI_ADDRESS
        port = PORT
        sock = connect_to_server(host,port)
        
        previous_X = -1
        previous_Y = -1
        robot_score = 0
        player_score = 0

        while True:
            run = True
            message = Message("GET")
            message.data.update(server_data)
            start = time.perf_counter()
            sock.sendall(message.encode_to_send(True))
            recv_data = sock.recv(1024)
            received = Message("RECEIVED")
            received.decode_from_receive(recv_data)

            if "error" in received.data:
                print(received.data["error"])
                run = False
            else:
                for key in received.data:
                    if received.data[key] == "not found" and key != "action":
                        print("Server did not return all required data. MISSING: %s"%key)
                        run = False

            if run:
                if received.data["ball_x"] == -1:
                    if not (5 < previous_X < TABLE["length"] - 5 and 5 < previous_Y < TABLE["width"] - 5):
                        previous_X = -1
                        previous_Y = -1
                        ball_hidden = False
                    else:
                        ball_hidden = True
                else:
                    ball_hidden = False
                    if abs(previous_X - received.data["ball_x"]) > NOISE_THRESHOLD:
                        previous_X = received.data["ball_x"]
                    if abs(previous_Y - received.data["ball_y"]) > NOISE_THRESHOLD:
                        previous_Y = received.data["ball_y"]
                        
                if robot_score != received.data["robot_score"] or player_score != received.data["player_score"]:
                    previous_X = -1
                    previous_Y = -1
                    robot_score = received.data["robot_score"]
                    player_score = received.data["player_score"]
                
                output = await asyncio.gather(
                    compute_next_state(current_states[0],previous_X, previous_Y, GOAL_ROD, received.data["stop"], ball_hidden, kick_timeout[0], received.data["robot_goal_rod_displacement_current"], received.data["robot_goal_rod_angle_current"]),
                    compute_next_state(current_states[1],previous_X, previous_Y, TWO_ROD, received.data["stop"], ball_hidden, kick_timeout[2], received.data["robot_2_rod_displacement_current"], received.data["robot_2_rod_angle_current"]),
                    compute_next_state(current_states[2],previous_X, previous_Y, FIVE_ROD, received.data["stop"], ball_hidden, kick_timeout[4], received.data["robot_5_rod_displacement_current"], received.data["robot_5_rod_angle_current"]),
                    compute_next_state(current_states[3],previous_X, previous_Y, THREE_ROD, received.data["stop"], ball_hidden, kick_timeout[6], received.data["robot_3_rod_displacement_current"], received.data["robot_3_rod_angle_current"]),
                    compute_command(current_states[0], GOAL_ROD, previous_X, previous_Y, TABLE, received.data["robot_goal_rod_displacement_current"]),
                    compute_command(current_states[1], TWO_ROD, previous_X, previous_Y, TABLE, received.data["robot_2_rod_displacement_current"]),
                    compute_command(current_states[2], FIVE_ROD, previous_X, previous_Y, TABLE, received.data["robot_5_rod_displacement_current"]),
                    compute_command(current_states[3], THREE_ROD, previous_X, previous_Y, TABLE, received.data["robot_3_rod_displacement_current"])
                )               

                k = 0
                for i in range(len(output)):
                    if i < len(current_states):
                        current_states[i] = output[i]
                    else:
                        for j in range(2):
                            if output[i][j] != -1:
                                commands[list(commands.keys())[k]] = int(output[i][j])
                            k+=1
                            
                for i in range(len(current_states)):
                    if current_states[i] == "Kick" and not kick_timeout[i * 2 + 1]:
                        kick_timeout[i * 2 + 1] = True
                        kick_timeout[i * 2] = time.perf_counter()
                    elif current_states[i] != "Kick":
                        kick_timeout[i * 2 + 1] = False
                        kick_timeout[i * 2] = 0


                command_message = Message("POST")
                command_message.data.update(commands)
                sock.sendall(command_message.encode_to_send(True))

                #total_time += time.perf_counter() - start
                print(output)
                    
    except KeyboardInterrupt:
        print("caught keyboard interrupt, exiting")
    finally:
        sock.send(b'')
        sock.close()
if __name__ == '__main__':
    main()

asyncio.run(main())