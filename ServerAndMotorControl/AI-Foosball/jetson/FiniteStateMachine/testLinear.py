import socket

from Message import Message
from rodFSM import compute_rod_linear
from FSMConstants import *

def main():
    commands = {"robot_goal_rod_displacement_command":0, "robot_goal_rod_angle_command":0, "robot_2_rod_displacement_command":0, "robot_2_rod_angle_command":0, "robot_5_rod_displacement_command":0, "robot_5_rod_angle_command":0, "robot_3_rod_displacement_command":0, "robot_3_rod_angle_command":0}
    server_data = {"ball_x":0, "ball_y":0, "ball_Vx":0, "ball_Vy":0}

    try:
        host = PI_ADDRESS
        port = PORT       
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect((host, port))
        number_of_runs = 1

        while True:
            run = True
            #input("Press enter to continue")
            message = Message("GET")
            message.data.update(server_data)
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
                output = [compute_rod_linear(GOAL_ROD, received.data["ball_y"]),compute_rod_linear(TWO_ROD, received.data["ball_y"]),compute_rod_linear(FIVE_ROD, received.data["ball_y"]),compute_rod_linear(THREE_ROD, received.data["ball_y"])]
                #output = [GOAL_ROD["maxActuation"] * mumber_of_runs * 0.33, TWO_ROD["maxActuation"] * mumber_of_runs * 0.33, FIVE_ROD["maxActuation"] * mumber_of_runs * 0.33, THREE_ROD["maxActuation"] * mumber_of_runs * 0.33]
                
                for i in range(len(output)):
                    commands[list(commands.keys())[i*2]] = int(output[i])
                
                print(received.data)
                print(commands)
                command_message = Message("POST")
                command_message.data.update(commands)
                sock.sendall(command_message.encode_to_send(True))
                number_of_runs += 1
    except KeyboardInterrupt:
        print("caught keyboard interrupt, exiting")
    finally:
        print("socket closed")
        sock.send(b'')
        sock.close()
if __name__ == '__main__':
    main()