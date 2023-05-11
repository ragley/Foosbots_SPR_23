using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Scripting.Python;
using UnityEditor;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


public class DummyInference : MonoBehaviour
{


    public Rigidbody rb;
    Vector3 rot_Force;
    public DummyAgent nn;
    public float previous_X = -1f;
    public float previous_Y = -1f;
    public int robot_score = 0;
    public int player_score = 0;
    public bool run = true;
    public bool ball_hidden;
    public string host = Constants.PI_ADDRESS;
    public int port = Constants.PORT;
    public Socket socket;

    public float[] inputs = new float[46];
    public float[] decisions = new float[8];
    public float[] output = new float[8];
    public Dictionary<string, object> commands = new Dictionary<string, object>
    {
        {"robot_goal_rod_displacement_command", 0},
        {"robot_goal_rod_angle_command", 0},
        {"robot_2_rod_displacement_command", 0},
        {"robot_2_rod_angle_command", 0},
        {"robot_5_rod_displacement_command", 0},
        {"robot_5_rod_angle_command", 0},
        {"robot_3_rod_displacement_command", 0},
        {"robot_3_rod_angle_command", 0}
    };

    public Dictionary<string, object> server_data = new Dictionary<string, object>
    {
        {"player_score", 0},
        {"robot_score", 0},
        {"stop", false},
        {"ball_x", 0},
        {"ball_y", 0},
        {"ball_Vx", 0},
        {"ball_Vy", 0},
        {"robot_goal_rod_displacement_current", 0},
        {"robot_goal_rod_angle_current", 0},
        {"robot_2_rod_displacement_current", 0},
        {"robot_2_rod_angle_current", 0},
        {"robot_5_rod_displacement_current", 0},
        {"robot_5_rod_angle_current", 0},
        {"robot_3_rod_displacement_current", 0},
        {"robot_3_rod_angle_current", 0}
    };

    public Socket ConnectToServer(string host, int port)
    {
        Socket sock;
        while (true)
        {
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(host, port);
                print("comnbnected");
                return sock;
            }
            catch
            {
                // Unity freezes when it doesn't connect on first try not sure why
                print("Cannot find server... not trying again...");
                Thread.Sleep(2000); // sleep for 2 seconds
                return null;
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // RB for visuals 
        rb = GetComponent<Rigidbody>();
        socket = ConnectToServer(host,port);

    }
    
    void FixedUpdate()
    {
       
        rot_Force = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
        rb.AddTorque(rot_Force, ForceMode.VelocityChange);
        
        Message message = new Message("GET");
        message.UpdateRods(server_data);
        //var start = Stopwatch.StartNew();
        socket.Send(message.EncodeToSend(true));
        byte[] recvData = new byte[1024];
        socket.Receive(recvData);
        Message received = new Message("RECEIVED");
        received.DecodeFromReceive(Encoding.UTF8.GetString(recvData));
        
        //print(received.data);

        if (received.data.ContainsKey("error"))
        {
            print(received.data["error"]);
            run = false;
        }
        else
        {
            foreach (string key in received.data.Keys)
            {
                //print(key + " " + received.data[key]);
                //print();
                if (received.data[key].ToString() == "not found" && key.ToString() != "action")
                {
                    print("Server did not return all required data. MISSING: " + key);
                    run = false;
                }
            }
        }

        if (run)
        {
        
            previous_Y = System.Convert.ToSingle(received.data["ball_y"]);
            previous_X = System.Convert.ToSingle(received.data["ball_x"]);
            if (robot_score != System.Convert.ToInt32(received.data["robot_score"]) || player_score != System.Convert.ToInt32(received.data["player_score"]))
            {
                nn.EndEpisode();

                previous_X = -1;
                previous_Y = -1;
                Thread.Sleep(6000);
                robot_score = System.Convert.ToInt32(received.data["robot_score"]);
                player_score = System.Convert.ToInt32(received.data["player_score"]);
            }


            // Assign inputs to values received from the server
            // Some z had to be multiplied by negative 1 cause of how the system was set up on the new table
            // Three Rod / Attack Rod 
            float rod3x = Constants.ThreeRod.rodX;
            float rod3z = System.Convert.ToSingle(received.data["robot_3_rod_displacement_current"]);
            float rod3P0z = Constants.MIN_PLAYER_OFFSET + rod3z;
            float rod3P1z = Constants.MIN_PLAYER_OFFSET + rod3z + Constants.ThreeRod.playerSpacing;
            float rod3P2z = Constants.MIN_PLAYER_OFFSET + rod3z + 2 * Constants.ThreeRod.playerSpacing;
            
            float rod3P0x = Constants.ThreeRod.rodX; 
            float rod3P1x = Constants.ThreeRod.rodX;
            float rod3P2x = Constants.ThreeRod.rodX;

            // add difference between unity output and converters to make sure inf on table sees the same
            float[] rod3Obs = { 
                Converters.IRL_2_U_X(rod3x) + Constants.ThreeRod.X_Correction, 
                Constants.ThreeRod.Z_Correction(Converters.IRL_2_U_Z(rod3z)), 
                System.Convert.ToSingle(received.data["robot_3_rod_angle_current"]) * -1, 
                Converters.IRL_2_U_X(rod3P0x) + Constants.ThreeRod.X_Correction, 
                Constants.ThreeRod.Z_Correction_P0(Converters.IRL_2_U_Z(rod3P0z) * -1), 
                Converters.IRL_2_U_X(rod3P1x) + Constants.ThreeRod.X_Correction, 
                Constants.ThreeRod.Z_Correction_P1(Converters.IRL_2_U_Z(rod3P1z) * -1), 
                Converters.IRL_2_U_X(rod3P2x) + Constants.ThreeRod.X_Correction, 
                Constants.ThreeRod.Z_Correction_P2(Converters.IRL_2_U_Z(rod3P2z) * -1) 
            };


            // Midfield / 5 ROD
            float rod5x = Constants.FiveRod.rodX;
            float rod5z = System.Convert.ToSingle(received.data["robot_5_rod_displacement_current"]);
            float rod5P0z = Constants.MIN_PLAYER_OFFSET + rod5z;
            float rod5P1z = Constants.MIN_PLAYER_OFFSET + rod5z + Constants.FiveRod.playerSpacing;
            float rod5P2z = Constants.MIN_PLAYER_OFFSET + rod5z + 2 * Constants.FiveRod.playerSpacing;
            float rod5P3z = Constants.MIN_PLAYER_OFFSET + rod5z + 3 * Constants.FiveRod.playerSpacing;
            float rod5P4z = Constants.MIN_PLAYER_OFFSET + rod5z + 4 * Constants.FiveRod.playerSpacing;
            float rod5P0x = Constants.FiveRod.rodX;
            float rod5P1x = Constants.FiveRod.rodX;
            float rod5P2x = Constants.FiveRod.rodX;
            float rod5P3x = Constants.FiveRod.rodX;
            float rod5P4x = Constants.FiveRod.rodX;

            float[] rod5Obs = { 
                Converters.IRL_2_U_X(rod5x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction(Converters.IRL_2_U_Z(rod5z)), 
                System.Convert.ToSingle(received.data["robot_5_rod_angle_current"]) * -1, 
                Converters.IRL_2_U_X(rod5P0x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction_P0(Converters.IRL_2_U_Z(rod5P0z) * -1), 
                Converters.IRL_2_U_X(rod5P1x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction_P1(Converters.IRL_2_U_Z(rod5P1z) * -1), 
                Converters.IRL_2_U_X(rod5P2x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction_P2(Converters.IRL_2_U_Z(rod5P2z) * -1), 
                Converters.IRL_2_U_X(rod5P3x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction_P3(Converters.IRL_2_U_Z(rod5P3z) * -1), 
                Converters.IRL_2_U_X(rod5P4x) + Constants.FiveRod.X_Correction, 
                Constants.FiveRod.Z_Correction_P4(Converters.IRL_2_U_Z(rod5P4z) * -1)
            };


            // Two Rod / Defence Rod
            float rod2x = Constants.TwoRod.rodX;
            float rod2z = System.Convert.ToSingle(received.data["robot_2_rod_displacement_current"]);
            float rod2P0z = Constants.MIN_PLAYER_OFFSET + rod2z;
            float rod2P1z = rod2P0z + Constants.TwoRod.playerSpacing;
            float rod2P0x = Constants.TwoRod.rodX;
            float rod2P1x = Constants.TwoRod.rodX;

            float[] rod2Obs = new float[] {
                Converters.IRL_2_U_X(rod2x) + Constants.TwoRod.X_Correction,
                Constants.TwoRod.Z_Correction(Converters.IRL_2_U_Z(rod2z)),
                System.Convert.ToSingle(received.data["robot_2_rod_angle_current"]) * -1,
                Converters.IRL_2_U_X(rod2P0x) + Constants.TwoRod.X_Correction,
                Constants.TwoRod.Z_Correction_P0(Converters.IRL_2_U_Z(rod2P0z) * -1),
                Converters.IRL_2_U_X(rod2P1x) + Constants.TwoRod.X_Correction,
                Constants.TwoRod.Z_Correction_P1(Converters.IRL_2_U_Z(rod2P1z) * -1)
            };

            // Goal Rod / Goalkeeper Rod
            float rodgx = Constants.GoalRod.rodX;
            float rodgz = System.Convert.ToSingle(received.data["robot_goal_rod_displacement_current"]);
            float rodgP0z = Constants.MIN_PLAYER_OFFSET + rodgz;
            float rodgP1z = Constants.MIN_PLAYER_OFFSET + rodgz + Constants.GoalRod.playerSpacing;
            float rodgP2z = Constants.MIN_PLAYER_OFFSET + rodgz + 2 * Constants.GoalRod.playerSpacing;
            float rodgP0x = Constants.GoalRod.rodX;
            float rodgP1x = Constants.GoalRod.rodX;
            float rodgP2x = Constants.GoalRod.rodX;

            float[] rodgObs = new float[] {
                Converters.IRL_2_U_X(rodgx) + Constants.GoalRod.X_Correction,
                Constants.GoalRod.Z_Correction(Converters.IRL_2_U_Z(rodgz)),
                System.Convert.ToSingle(received.data["robot_goal_rod_angle_current"]) * -1,
                Converters.IRL_2_U_X(rodgP0x) + Constants.GoalRod.X_Correction,
                Constants.GoalRod.Z_Correction_P0(Converters.IRL_2_U_Z(rodgP0z) * -1),
                Converters.IRL_2_U_X(rodgP1x) + Constants.GoalRod.X_Correction,
                Constants.GoalRod.Z_Correction_P1(Converters.IRL_2_U_Z(rodgP1z) * -1),
                Converters.IRL_2_U_X(rodgP2x) + Constants.GoalRod.X_Correction,
                Constants.GoalRod.Z_Correction_P2(Converters.IRL_2_U_Z(rodgP2z) * -1)
            };

            // Irl to Unity calc done in ball_tracking.py
            float[] ballObs = new float[] {
                System.Convert.ToSingle(received.data["ball_x"]),
                System.Convert.ToSingle(received.data["ball_y"]),
                System.Convert.ToSingle(received.data["ball_Vx"]), 
                System.Convert.ToSingle(received.data["ball_Vy"])
            };

            float[] goalObs = new float[] {
                Converters.IRL_2_U_X(Constants.Table.robot_goalX),
                0,
                Converters.IRL_2_U_X(Constants.Table.player_goalX) + 0.74107f,
                0
            };
            
            int i = 0;

            foreach (float obs in ballObs) {
                inputs[i] = obs;
                i++;
            }

            foreach (float obs in rod3Obs) {
                inputs[i] = obs;
                i++;
            }

            foreach (float obs in rod5Obs) {
                inputs[i] = obs;
                i++;
            }

            foreach (float obs in rod2Obs) {
                inputs[i] = obs;
                i++;
            }

            foreach (float obs in rodgObs) {
                inputs[i] = obs;
                i++;
            }

            foreach (float obs in goalObs) {
                inputs[i] = obs;
                i++;
            }

            
            decisions = nn.actions;

            output[0] = Converters.ComputeRodLinear(Constants.GoalRod.maxActuation, Constants.GoalRod.playerSpacing, Converters.NN_OP_2_IRL_LIN(Constants.GoalRod.maxActuation, decisions[4]));
            output[1] = Converters.NN_OP_2_IRL_ROT(decisions[5]) * -1;
            output[2] = Converters.ComputeRodLinear(Constants.TwoRod.maxActuation, Constants.TwoRod.playerSpacing, Converters.NN_OP_2_IRL_LIN(Constants.TwoRod.maxActuation, decisions[2]));
            output[3] = Converters.NN_OP_2_IRL_ROT(decisions[3]) * -1;
            output[4] = Converters.ComputeRodLinear(Constants.FiveRod.maxActuation, Constants.FiveRod.playerSpacing, Converters.NN_OP_2_IRL_LIN(Constants.FiveRod.maxActuation, decisions[6]));
            output[5] = Converters.NN_OP_2_IRL_ROT(decisions[7]) * -1;
            output[6] = Converters.ComputeRodLinear(Constants.ThreeRod.maxActuation, Constants.ThreeRod.playerSpacing, Converters.NN_OP_2_IRL_LIN(Constants.ThreeRod.maxActuation, decisions[0]));
            output[7] = Converters.NN_OP_2_IRL_ROT(decisions[1]) * -1;

            for (int k = 0; k < output.Length; k++)
            {
                string key = "robot_goal_rod_displacement_command"; // Dictionary key
                switch (k)
                {
                    case 0:
                        key = "robot_goal_rod_displacement_command";
                        break;
                    case 1:
                        key = "robot_goal_rod_angle_command";
                        break;
                    case 2:
                        key = "robot_2_rod_displacement_command";
                        break;
                    case 3:
                        key = "robot_2_rod_angle_command";
                        break;
                    case 4:
                        key = "robot_5_rod_displacement_command";
                        break;
                    case 5:
                        key = "robot_5_rod_angle_command";
                        break;
                    case 6:
                        key = "robot_3_rod_displacement_command";
                        break;
                    case 7:
                        key = "robot_3_rod_angle_command";
                        break;
                    default:
                        break;
                }

                // Set the value from the array to the dictionary
                commands[key] = output[k];
            }

            Message command_message = new Message("POST");
            command_message.UpdateRods(commands);
            socket.Send(command_message.EncodeToSend(true));

            //Thread.Sleep(500);

        }

    }

}
