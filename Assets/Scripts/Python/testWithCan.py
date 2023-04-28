import socket
import time
from Message import Message

commands = {"robot_goal_rod_displacement_command":0, "robot_goal_rod_angle_command":0, "robot_two_rod_displacement_command":0, "robot_two_rod_angle_command":0, "robot_five_rod_displacement_command":0, "robot_five_rod_angle_command":0, "robot_three_rod_displacement_command":0, "robot_three_rod_angle_command":0}
server_data = {"robot_goal_rod_displacement_current":0, "robot_goal_rod_angle_current":0, "robot_two_rod_displacement_current":0, "robot_two_rod_angle_current":0, "robot_five_rod_displacement_current":0, "robot_five_rod_angle_current":0, "robot_three_rod_displacement_current":0, "robot_three_rod_angle_current":0}
try:
    HOST = '127.0.0.1'  # The server's hostname or IP address
    PORT = 5000       # The port used by the server
    number_of_runs = 0
    #total_time = 0.0
    
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))

    while True:
        message = Message("GET")
        message.data.update(server_data)
        start = time.perf_counter()
        print(message.data)
        sock.sendall(message.encode_to_send(True))
        recv_data = sock.recv(1024)
        received = Message("RECEIVED")
        received.decode_from_receive(recv_data)

        if "error" in received.data:
            print(received.data["error"])
        else:
            for key in received.data:
                if received.data[key] == "not found" and key != "action":
                    print("Server did not return all required data. MISSING: %s"%key)

        j = 0
        for key in commands:
            if j % 2 == 0:
                commands[key] = 400
            else:
                commands[key] = 0
            j+=1

        command_message = Message("POST")
        command_message.data.update(commands)
        sock.sendall(command_message.encode_to_send(True))
        #total_time += time.perf_counter() - start
        number_of_runs += 1
        #print(total_time / number_of_runs)
except KeyboardInterrupt:
    print("caught keyboard interrupt, exiting")
finally:
    print("socket closed")
    sock.send(b'')
    sock.close()