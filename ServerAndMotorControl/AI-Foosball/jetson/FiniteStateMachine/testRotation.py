import socket

from Message import Message
from FSMConstants import *

def main():
    try:
        host = PI_ADDRESS
        port = PORT       
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect((host, port))

        angles = [0, 45, 90, 180, -90, -45]
        count = 0

        while True:
            input("press Enter to continue")
            index = count % len(angles)
            commands = {"robot_goal_rod_displacement_command":GOAL_ROD["maxActuation"] / 2, "robot_goal_rod_angle_command":angles[index], "robot_2_rod_displacement_command":TWO_ROD["maxActuation"] / 2, "robot_2_rod_angle_command":angles[index], "robot_5_rod_displacement_command":FIVE_ROD["maxActuation"] / 2, "robot_5_rod_angle_command":angles[index], "robot_3_rod_displacement_command":THREE_ROD["maxActuation"] / 2, "robot_3_rod_angle_command":angles[index]}    

            print("Set angle is ",angles[index])
            command_message = Message("POST")
            command_message.data.update(commands)
            sock.sendall(command_message.encode_to_send(True))
            count += 1
    except KeyboardInterrupt:
        print("caught keyboard interrupt, exiting")
    finally:
        print("socket closed")
        sock.send(b'')
        sock.close()
if __name__ == '__main__':
    main()