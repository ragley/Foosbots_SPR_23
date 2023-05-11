import os
import socket
import can
import time
import threading
import struct
from Message import Message

#Initilizes socket information
HOST = "192.168.0.1"
PORT = 5000
SOCKET1 = None
SOCKET2 = None
CAN0 = None

def connectToServer():
    while True:
        try:
            #connects to the server
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.connect((HOST, PORT))                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
            return sock
        except:
            time.sleep(2)
            continue
        
def initalizeCan():
    hardware_connect = {}
    global SOCKET1
    while True:
        try:
            #Closes and opens the CAN communication
            os.system('sudo ifconfig can0 down')
            os.system('sudo ip link set can0 type can bitrate 1000000')
            os.system('sudo ifconfig can0 up')
            
            #Initilizes the CAN
            can0 = can.interface.Bus(channel = 'can0', bustype = 'socketcan')

            hardware_connect["USB2CAN"] = "Connection Successful"
            #Tells the server there is a conncetion
            message = Message("POST")
            message.data.update(hardware_connect)
            SOCKET1.sendall(message.encode_to_send(True))
            hardware_connect={}
            return can0
        except:
            hardware_connect["USB2CAN"] = "Could not connect to CAN bus"
            #sends exception to the server
            message = Message("POST")
            message.data.update(hardware_connect)
            SOCKET1.sendall(message.encode_to_send(True))
            
            hardware_connect={}
            time.sleep(2)
            continue

def receiveCan():
    global SOCKET1
    global CAN0
    #update flag for the server
    update_server = False
    #stop flag to tell if the rods are stopped
    stop = True
    #initialize score count
    player_score = 0
    robot_score = 0
    #Dictionary for the server, current position
    rods_current = {}
    
    #Hex values of all binary values 
    #41 42 44 48
    #81 82 84 88
    #b0 20
    #Dictionary for CAN
    Controller = {
        0b01001000: {"Name": "Zero_goal"},
        0b01000100: {"Name": "Zero_2"},
        0b01000010: {"Name": "Zero_5"},
        0b01000001: {"Name": "Zero_3"},
        0b10011000: {"Name": "Controller_goal"},
        0b10010100: {"Name": "Controller_2"},
        0b10010010: {"Name": "Controller_5"},
        0b10010001: {"Name": "Controller_3"},
        0b10001000: {"Name": "Controller_goal_stop"},
        0b10000100: {"Name": "Controller_2_stop"},
        0b10000010: {"Name": "Controller_5_stop"},
        0b10000001: {"Name": "Controller_3_stop"},
        0b10110000: {"Name": "Goal_update"},
        0b00100000: {"Name": "Goal_reset"},
    }

    
    while True:
        #Received message with 0 second timeout
        msg1 = CAN0.recv(0.0)

        if msg1 is not None:
            id = msg1.arbitration_id
            update_server = True
            if id in Controller.keys():
                name = Controller[msg1.arbitration_id]["Name"]
                #splits the data into 2 bytearrays 
                split1 = msg1.data[0:4]
                split2 = msg1.data[4:8]
                if name == "Goal_update":
                    #data1 is player score, data2 is robot score
                    data1 = struct.unpack('>l',split1)[0]
                    data2 = struct.unpack('>l',split2)[0]
                else:
                    #data1 is linear position, data2 is angle position
                    data1 = struct.unpack('>f',split1)[0]
                    data2 = struct.unpack('>f',split2)[0]
                    
                #Non-stop messages
                if name == "Controller_goal":
                    rods_current["robot_goal_rod_displacement_current"] = data1
                    rods_current["robot_goal_rod_angle_current"] = data2
                    stop = False
                elif name == "Controller_2":
                    rods_current["robot_2_rod_displacement_current"] = data1
                    rods_current["robot_2_rod_angle_current"] = data2
                    stop = False
                elif name == "Controller_5":
                    rods_current["robot_5_rod_displacement_current"] = data1
                    rods_current["robot_5_rod_angle_current"] = data2
                    stop = False
                elif name == "Controller_3":
                    rods_current["robot_3_rod_displacement_current"] = data1
                    rods_current["robot_3_rod_angle_current"] = data2
                    stop = False
                #Stop messages    
                elif name == "Controller_goal_stop":
                    rods_current["robot_goal_rod_displacement_current"] = data1
                    rods_current["robot_goal_rod_angle_current"] = data2
                    stop = True
                elif name == "Controller_2_stop":
                    rods_current["robot_2_rod_displacement_current"] = data1
                    rods_current["robot_2_rod_angle_current"] = data2
                    stop = True
                elif name == "Controller_5_stop":
                    rods_current["robot_5_rod_displacement_current"] = data1
                    rods_current["robot_5_rod_angle_current"] = data2
                    stop = True
                elif name == "Controller_3_stop":
                    rods_current["robot_3_rod_displacement_current"] = data1
                    rods_current["robot_3_rod_angle_current"] = data2
                    stop = True
                #Goal messages
                elif name == "Goal_update":
                    player_score = data1
                    robot_score = data2
                elif name == "Goal_reset":
                    player_score = 0  
                    robot_score = 0
                rods_current["player_score"] = player_score
                rods_current["robot_score"] = robot_score
                rods_current["stop"] = stop
                
            else:
                #Tells server an unexpected message arrived
                #sends hex ID with raw data of the message as a string
                data = "0x"
                for byte in msg1.data:
                    raw_data = str(byte)
                    formatted_data = raw_data.replace("0x","")
                    data += formatted_data
                    
                rods_current[hex(msg1.arbitration_id)] = data
                
        if update_server:        
            message = Message("POST")
            message.data.update(rods_current)
            SOCKET1.sendall(message.encode_to_send(True))
            rods_current={}
            update_server = False

def sendCan():
    global SOCKET2
    global CAN0
    #sets a timer to send CAN messages every 20 miliseconds
    timer = time.perf_counter() * 1000
    #delay in milliseconds to send to the CAN
    delay = 10
    #flags to tell if the game is started or stopped
    game_flag = False
    game_stop = True
    #local variable for current score
    player_score = 0
    robot_score = 0
    #Dictionary for the server
    rods_desired = {
    "robot_goal_rod_displacement_command": 0,
    "robot_goal_rod_angle_command": 0,
    "robot_2_rod_displacement_command": 0,
    "robot_2_rod_angle_command": 0,
    "robot_5_rod_displacement_command": 0,
    "robot_5_rod_angle_command": 0,
    "robot_3_rod_displacement_command": 0,
    "robot_3_rod_angle_command": 0,
    "game_flag": 0,
    "pause": 0,
    "player_score": 0,
    "robot_score": 0
    }
    
    while True:
        run = True
        message = Message("GET")
        message.data.update(rods_desired)
        SOCKET2.sendall(message.encode_to_send(True))
        recv_data = SOCKET2.recv(1024)
        received = Message("RECEIVED")
        received.decode_from_receive(recv_data)
        #checks for error messages from the server
        if "error" in received.data:
                print(received.data["error"])
                run = False
        else:
            for key in received.data:
                if received.data[key] == "not found" and key != "action":
                    run = False
        
        if "game_flag" in received.data:
            game_flag = received.data["game_flag"]
            #When the game starts, send a reset signal to the CAN
            if game_flag and game_stop:
                msg_game_reset = can.Message(arbitration_id = 0b01001111, data=[0,0,0,0,0,0,0,0], is_extended_id=False)
                msg_goal_reset = can.Message(arbitration_id = 0b00100000, data=[0,0,0,0,0,0,0,0], is_extended_id=False)
                CAN0.send(msg_game_reset)
                CAN0.send(msg_goal_reset)
                time.sleep(0.01) #Sends another message in case it gets dropped
                CAN0.send(msg_game_reset)
                CAN0.send(msg_goal_reset)
                game_stop = False
            #When the game stops, stop sending signals to the CAN  
            elif not game_flag and not game_stop:
                game_stop = True
                
        #Sends a reset signal when a goal is scored
        if not( player_score == received.data["player_score"]):
            msg_game_reset = can.Message(arbitration_id = 0b01001111, data=[0,0,0,0,0,0,0,0], is_extended_id=False)
            CAN0.send(msg_game_reset)
            time.sleep(0.01)  #Sends another message in case it gets dropped
            CAN0.send(msg_game_reset)
            player_score = received.data["player_score"]
        if not( robot_score == received.data["robot_score"]):
            msg_game_reset = can.Message(arbitration_id = 0b01001111, data=[0,0,0,0,0,0,0,0], is_extended_id=False)
            CAN0.send(msg_game_reset)
            time.sleep(0.01)  #Sends another message in case it gets dropped
            CAN0.send(msg_game_reset)
            robot_score = received.data["robot_score"]
            
            
                
        #Checks for no errors from the server and checks for the game to be started
        if run:                 
            #When the game is being played and is not paused, send commands to CAN
            if game_flag and not game_stop and not received.data["pause"] :
                #sends signals to the CAN on a delay in miliseconds
                if(time.perf_counter()*1000 - timer > delay):
                    #print(time.perf_counter()*1000 - timer)
                    timer = time.perf_counter()*1000
                    
                    goal_data1 = bytearray(struct.pack('>f',received.data["robot_goal_rod_displacement_command"]))
                    goal_data2 = bytearray(struct.pack('>f',received.data["robot_goal_rod_angle_command"]))
                    
                    rod2_data1 = bytearray(struct.pack('>f',received.data["robot_2_rod_displacement_command"]))
                    rod2_data2 = bytearray(struct.pack('>f',received.data["robot_2_rod_angle_command"]))
                    
                    rod5_data1 = bytearray(struct.pack('>f',received.data["robot_5_rod_displacement_command"]))
                    rod5_data2 = bytearray(struct.pack('>f',received.data["robot_5_rod_angle_command"]))
                    
                    rod3_data1 = bytearray(struct.pack('>f',received.data["robot_3_rod_displacement_command"]))
                    rod3_data2 = bytearray(struct.pack('>f',received.data["robot_3_rod_angle_command"]))
                    
                    msg_goal = can.Message(arbitration_id = 0b00011000, data=goal_data1 + goal_data2, is_extended_id=False)
                    msg_2 = can.Message(arbitration_id = 0b00010100, data=rod2_data1 + rod2_data2, is_extended_id=False)
                    msg_5 = can.Message(arbitration_id = 0b00010010, data=rod5_data1 + rod5_data2, is_extended_id=False)
                    msg_3 = can.Message(arbitration_id = 0b00010001, data=rod3_data1 + rod3_data2, is_extended_id=False)
                    
                    CAN0.send(msg_goal)
                    CAN0.send(msg_2)
                    CAN0.send(msg_5)
                    CAN0.send(msg_3)
         

try:
    SOCKET1 = connectToServer()
    SOCKET2 = connectToServer()
    CAN0 = initalizeCan()
    t1 = threading.Thread(target=receiveCan)
    t2 = threading.Thread(target=sendCan)
    t1.start()
    t2.start()
    while True:
        t1.join()
        t2.join()
except KeyboardInterrupt:
    print("Exiting")
finally:
    SOCKET1.send(b'')
    SOCKET1.close()
    SOCKET2.send(b'')
    SOCKET2.close()
    os.system('sudo ifconfig CAN0 down')

