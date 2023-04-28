from curses.ascii import TAB
import socket
import time
import asyncio
import onnx
import onnxruntime as ort
import numpy as np
from onnxruntime.capi.onnxruntime_pybind11_state import InvalidArgument
from time import sleep
from FSMConstants import *
from rodFSM import compute_rod_linear
from Message import Message
from converters import *

# Load the onnx modelfile:///run/media/foosbots/4C92BC1E92BC0E88/Foosball Sim/Foosbots_SPR_23/results/testwUI/teswUIbrain.onnx
#model = onnx.load("teswUIbrain.onnx")
# model = onnx.load("JT2.onnx")

# onnx.checker.check_model(model)

# Set up session for Inference
# RN model runs on cpu have to get it running on gpu idk how yet
# TODO: CUDAExecutionProvider
# sess = ort.InferenceSession("teswUIbrain.onnx", providers=['CPUExecutionProvider'])
# sess = ort.InferenceSession("JT2.onnx", providers=['CPUExecutionProvider'])

# Number of observational values from Unity
vectorOBSVal = 46

# Some checking on the inputs and stuff in inputs of the model idk
# input_name = sess.get_inputs()[0].name
# print(input_name)
# label_name = sess.get_outputs()[0].name
# label_name2 = sess.get_outputs()[2].name
# label_name4 = sess.get_outputs()[4].name
# print(label_name)
# print(label_name2)
# print(label_name4)



#time.sleep(0.5)
def connect_to_server(host, port):
    # Try to connect to the server
	while True:
		try:
			sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
			sock.connect((host, port))
			return sock
		except:
			print("Cannot find server... trying again...")
			time.sleep(2)
			continue

# Rotational Normalization to [-180,180]
def normRot(input):
    return (input * 180)

async def main():

    inputs = np.zeros(vectorOBSVal)
    output = [0, 0, 0, 0, 0, 0, 0, 0]
    nnOutputs = [0, 0, 0, 0, 0, 0, 0, 0]

    # Commands and server data initialization
    commands = {"robot_goal_rod_displacement_command":0, "robot_goal_rod_angle_command":0, "robot_2_rod_displacement_command":0, "robot_2_rod_angle_command":0, "robot_5_rod_displacement_command":0, "robot_5_rod_angle_command":0, "robot_3_rod_displacement_command":0, "robot_3_rod_angle_command":0}
    server_data = {"player_score":0, "robot_score":0, "stop":False, "ball_x":0, "ball_y":0, "ball_Vx":0, "ball_Vy":0, "robot_goal_rod_displacement_current":0, "robot_goal_rod_angle_current":0, "robot_2_rod_displacement_current":0, "robot_2_rod_angle_current":0, "robot_5_rod_displacement_current":0, "robot_5_rod_angle_current":0, "robot_3_rod_displacement_current":0, "robot_3_rod_angle_current":0}

    try:
        # Connect to the PI
        host = PI_ADDRESS
        port = PORT
        sock = connect_to_server(host,port)

        # Initialize game variables
        previous_X = -1
        previous_Y = -1
        robot_score = 0
        player_score = 0

        # Main loop to get data from the server
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

            # this is same as brainFSM, for ball being hidden, may change later to add memory of last positon it was seen
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
                    time.sleep(6)
                    robot_score = received.data["robot_score"]
                    player_score = received.data["player_score"]

                # Assign inputs to values recieved from the server
                
                # Three Rod / Attack Rod 
                rod3x = THREE_ROD["rodX"]
                rod3z = received.data["robot_3_rod_displacement_current"]
                rod3P0z = MIN_PLAYER_OFFSET + rod3z
                rod3P1z = MIN_PLAYER_OFFSET + rod3z + THREE_ROD["playerSpacing"]
                rod3P2z = MIN_PLAYER_OFFSET + rod3z + 2 * THREE_ROD["playerSpacing"]
                rod3P0x = rod3P1x = rod3P2x = THREE_ROD["rodX"]

                rod3Obs = [IRL_2_U_X(rod3x), IRL_2_U_Z(rod3z), received.data["robot_3_rod_angle_current"], IRL_2_U_X(rod3P0x), IRL_2_U_Z(rod3P0z), IRL_2_U_X(rod3P1x), IRL_2_U_Z(rod3P1z), IRL_2_U_X(rod3P2x), IRL_2_U_Z(rod3P2z)]


                # Midfield / 5 ROD
                rod5x = FIVE_ROD["rodX"]
                rod5z = received.data["robot_5_rod_displacement_current"]
                rod5P0z = MIN_PLAYER_OFFSET + rod5z
                rod5P1z = MIN_PLAYER_OFFSET + rod5z + FIVE_ROD["playerSpacing"]
                rod5P2z = MIN_PLAYER_OFFSET + rod5z + 2 * FIVE_ROD["playerSpacing"]
                rod5P3z = MIN_PLAYER_OFFSET + rod5z + 3 * FIVE_ROD["playerSpacing"]
                rod5P4z = MIN_PLAYER_OFFSET + rod5z + 4 * FIVE_ROD["playerSpacing"]
                rod5P0x = rod5P1x = rod5P2x = rod5P3x = rod5P4x = FIVE_ROD["rodX"]

                rod5Obs = [IRL_2_U_X(rod5x), IRL_2_U_Z(rod5z), received.data["robot_5_rod_angle_current"], IRL_2_U_X(rod5P0x), IRL_2_U_Z(rod5P0z), IRL_2_U_X(rod5P1x), IRL_2_U_Z(rod5P1z), IRL_2_U_X(rod5P2x), IRL_2_U_Z(rod5P2z), IRL_2_U_X(rod5P3x), IRL_2_U_Z(rod5P3z), IRL_2_U_X(rod5P4x), IRL_2_U_Z(rod5P4z)]
                

                # Two Rod / Defence Rod 
                rod2x = TWO_ROD["rodX"]
                rod2z = received.data["robot_2_rod_displacement_current"]
                rod2P0z = MIN_PLAYER_OFFSET + rod2z
                rod2P1z = rod2P0z + TWO_ROD["playerSpacing"]
                rod2P0x = rod2P1x = TWO_ROD["rodX"]

                rod2Obs = [IRL_2_U_X(rod2x), IRL_2_U_Z(rod2z), received.data["robot_2_rod_angle_current"], IRL_2_U_X(rod2P0x), IRL_2_U_Z(rod2P0z), IRL_2_U_X(rod2P1x), IRL_2_U_Z(rod2P1z)]

                
                # Goal Rod / Goalkeeper Rod 
                rodgx = GOAL_ROD["rodX"]
                rodgz = received.data["robot_goal_rod_displacement_current"]
                rodgP0z = MIN_PLAYER_OFFSET + rodgz
                rodgP1z = MIN_PLAYER_OFFSET + rodgz + GOAL_ROD["playerSpacing"]
                rodgP2z = MIN_PLAYER_OFFSET + rodgz + 2 * GOAL_ROD["playerSpacing"]
                rodgP0x = rodgP1x = rodgP2x = GOAL_ROD["rodX"]

                rodgObs = [IRL_2_U_X(rodgx), IRL_2_U_Z(rodgz), received.data["robot_goal_rod_angle_current"], IRL_2_U_X(rodgP0x), IRL_2_U_Z(rodgP0z), IRL_2_U_X(rodgP1x), IRL_2_U_Z(rodgP1z), IRL_2_U_X(rodgP2x), IRL_2_U_Z(rodgP2z)]
                
                
                ballObs = [received.data["ball_x"], received.data["ball_y"], received.data["ball_Vx"], received.data["ball_Vx"]]
                
                goalObs = [IRL_2_U_X(TABLE["robot_goalX"]), 0, IRL_2_U_X(TABLE["player_goalX"]), 0]
                i = 0
                
                for j in range(len(ballObs)):
                    inputs[i] = ballObs[j]
                    i += 1
                
                for j in range(len(rod3Obs)):
                    inputs[i] = rod3Obs[j]
                    i += 1

                for j in range(len(rod5Obs)):
                    inputs[i] = rod5Obs[j]
                    i += 1
                
                for j in range(len(rod2Obs)):
                    inputs[i] = rod2Obs[j]
                    i += 1

                for j in range(len(rodgObs)):
                    inputs[i] = rodgObs[j]
                    i += 1
                
                for j in range(len(goalObs)):
                    inputs[i] = goalObs[j]
                    i += 1  
                
                print("===========V==== INPUTS ====V===========")    
                print(inputs)

                # Inference with the model with inputs
                # Size and index must match values in unity
                # TODO: Ensure values match the index they are given with unity idk what that is yet: order assigned, something else, etc.
                try:
                    nnOutputs = sess.run(None, {"obs_0": [inputs]})

                    # OUTPUTS, [array index], unity designation, CAN ident, movement type, range:

                    # [0] attack rod        (3Rod)      linearPos:  [-1, 1]
                    # [1] attack rod        (3Rod)      rotPos:     [-1, 1]
                    # [2] defence rod       (2Rod)      linearPos:  [-1, 1]
                    # [3] defence rod       (2Rod)      rotPos:     [-1, 1]
                    # [4] goalkeeper rod    (GoalRod)   linearPos:  [-1, 1]
                    # [5] goalkeeper rod    (GoalRod)   rotPos:     [-1, 1]
                    # [6] midfield rod      (5Rod)      linearPos:  [-1, 1]
                    # [7] midfield rod      (5Rod)      rotPos:     [-1, 1]

                    # If you want to print the outputs of the NN directly use this
                    # TODO: Could be refined to be neater with labels etc. its kindof odd
                    #for i in range(len(nnOutputs)):
                        #print(nnOutputs[i])
                        #print("1D {} value {}".format(i, nnOutputs[i]))
                        #for j in range(len(nnOutputs[i])):
                             #print("1D {} 2D {} value {}".format(i,j, nnOutputs[i][j]))

                # Exception if input doesn't match what model wants
                except (RuntimeError, InvalidArgument) as e:
                    print("ERROR with Shape={0} - {1}".format(np.array(inputs).shape, e))

                # idk if this is right at all lol
                # output gives two size 8 arrays at index 2 and 4 of nnOutputs
                # looking at onnx file one is continuous and one is discreet-continuous values
                # not sure which is which or which one to actually use
                # 4 doesnt seem to change much while 2 changes a lot

                # outputs from NN layout: ['version_number', 'memory_size', 'continuous_actions',
                #                          'continuous_action_output_shape', 'action', 'is_continuous_control',
                #                          'action_output_shape']

                op = nnOutputs[2][0]
                nnOpStr = "===========V=== NN OUTPUTS ===V==========="
                print(nnOpStr)
                print(nnOutputs[2][0])
                print(nnOutputs[4][0])
                #print(op)


                # Reorder to fit establish commad order TODO: Consistent Ordering
                # Convert NN outputs to IRL outputs usable by CAN
                output[0] = compute_rod_linear(GOAL_ROD, NN_OP_2_IRL_LIN(GOAL_ROD, op[4]))
                output[1] = NN_OP_2_IRL_ROT(op[5])
                output[2] = compute_rod_linear(TWO_ROD, NN_OP_2_IRL_LIN(TWO_ROD, op[2]))
                output[3] = NN_OP_2_IRL_ROT(op[3])
                output[4] = compute_rod_linear(FIVE_ROD, NN_OP_2_IRL_LIN(FIVE_ROD, op[6]))
                output[5] = NN_OP_2_IRL_ROT(op[7])
                output[6] = compute_rod_linear(THREE_ROD, NN_OP_2_IRL_LIN(THREE_ROD, op[0]))
                output[7] = NN_OP_2_IRL_ROT(op[1])
                OpStr = "===========V=== TABLE OUTPUTS ===V==========="
                print(OpStr)
                print(output)

                # Set the output of the NN to the commands for the CAN
                for i in range(len(output)):
                    commands[list(commands.keys())[i]] = int(output[i])

                # Print the Commands to the console to see what they are
                cmdVal = list(commands.values())
                cmdItem = list(commands.keys())
                cmdStr = "===========V=== COMMANDS ===V==========="

                for i in range(len(cmdItem)):
                    cmdStr = cmdStr + "\n" + str(cmdItem[i]) + ": " + str(cmdVal[i])

                print(cmdStr)

                # Post commands to the server
                command_message = Message("POST")
                command_message.data.update(commands)
                sock.sendall(command_message.encode_to_send(True))
                time.sleep(0.12)

    except KeyboardInterrupt:
        print("caught keyboard interrupt, exiting")

    finally:
        sock.send(b'')
        sock.close()


if __name__ == '__main__':
    main()

#asyncio.run(main())
loop = asyncio.get_event_loop()
loop.run_until_complete(main())
loop.close()
