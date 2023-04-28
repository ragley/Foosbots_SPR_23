using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // NETWORK 
    public const string LOCALHOST = "127.0.0.1";
    public const string PI_ADDRESS = "192.168.0.1";
    public const int PORT = 5000;

    // STATE
    public const int MOVEMENT_MARGIN = 2;
    public const int KICK_TIMEOUT = 1;
    public const int LAST_POSITION = -1;
    public const int PLAYER_LENGTH = 2;
    public const int NOISE_THRESHOLD = 3;
    public const int MIN_VELOCITY_THRESHOLD = 300;
    public const int OPEN_PREP_RANGE = -30;
    public const int BLOCK_PREP_RANGE = 100;
    public const int OPEN_KICK_RANGE = -20;
    public const int BLOCK_KICK_RANGE = 60;
    public const int KICK_ANGLE = 55;
    public const int PREP_ANGLE = -30;
    public const int BLOCK_ANGLE = 0;
    public const int OPEN_ANGLE = -90;
    public const int SPEED_THRESHOLD = 3000;
    public const int MIN_PLAYER_OFFSET = 47;
    public const int MAX_PLAYER_OFFSET = 640;
    public const int IDLE_RANGE = 600;
    public const int RECOVERY_LINEAR = 80;
    public const int RECOVERY_ANGLE = -57;

    // Unity
    // Perhaps these need to be more precisely calculated
    public const int U_TABLE_LENGTH_MAX = 76;
    public const int U_TABLE_LENGTH_MIN = 0;
    public const float U_TABLE_WIDTH_MAX = 1.5f;
    public const float U_TABLE_WIDTH_MIN = -1.5f;

    // Dimensions of the Table balltracking (these are VERY important) (mm)
    public const float TABLE_LENGTH = 1193.8f;
    public const float TABLE_WIDTH = 694.2f;

    //PHYSICAL DIMENSIONS
    public static class GoalRod
    {
        public const int maxActuation = 232;
        public const int playerSpacing = 183;
        public const int rodX = 1091;
        public const int numPlayers = 3;

        public const float X_Correction = 0.074552f;

        public const float Sim_Lin_Zero = 0f;
        public const float Sim_Lin_Max = 0.5118f;
        public const float Sim_Lin_Min = -0.5118f;

        // Min max values of observations sim recieves
        public const float Conv_Table_Obs_Zero = -1.000707f;
        public const float Conv_Table_Obs_Max = -0.4839104f; 
        public const float Conv_Table_Obs_Min = -1.5f;
        public static float Z_Correction(float z_pos)
        {
            float cor = ((z_pos - Conv_Table_Obs_Min)/(Conv_Table_Obs_Max - Conv_Table_Obs_Min)) * (Sim_Lin_Max - Sim_Lin_Min) + Sim_Lin_Min;
            return cor;
        }
    }

    public static class TwoRod
    {
        public const int maxActuation = 352;
        public const int playerSpacing = 235;
        public const int rodX = 947;
        public const int numPlayers = 2;

        public const float X_Correction = 0.17236f;

        public const float Sim_Lin_Zero = 0f;
        public const float Sim_Lin_Max = 0.717f;
        public const float Sim_Lin_Min = -0.717f;

        public const float Conv_Table_Obs_Zero = -0.7204443f;
        public const float Conv_Table_Obs_Max = 0.0483828f; 
        public const float Conv_Table_Obs_Min = -1.5f;
        public static float Z_Correction(float z_pos)
        {
            float cor = ((z_pos - Conv_Table_Obs_Min)/(Conv_Table_Obs_Max - Conv_Table_Obs_Min)) * (Sim_Lin_Max - Sim_Lin_Min) + Sim_Lin_Min;
            return cor;
        }

    }

    public static class FiveRod
    {
        public const int maxActuation = 113;
        public const int playerSpacing = 118;
        public const int rodX = 656;
        public const int numPlayers = 5;
        public const float X_Correction = 0.1902f;
        public const float Sim_Lin_Zero = 0f;
        public const float Sim_Lin_Max = 0.267f;
        public const float Sim_Lin_Min = -0.267f;

        public const float Conv_Table_Obs_Zero = -1.24878f;
        public const float Conv_Table_Obs_Max = -1.00288f; 
        public const float Conv_Table_Obs_Min = -1.5f;


        public static float Z_Correction(float z_pos)
        {
            float cor = ((z_pos - Conv_Table_Obs_Min)/(Conv_Table_Obs_Max - Conv_Table_Obs_Min)) * (Sim_Lin_Max - Sim_Lin_Min) + Sim_Lin_Min;
            return cor;
        }
    }

    public static class ThreeRod
    {
        public const int maxActuation = 228;
        public const int playerSpacing = 183;
        public const int rodX = 368;
        public const int numPlayers = 3;
        public const float X_Correction = 0.39364f;

        public const float Sim_Lin_Zero = 0f;
        public const float Sim_Lin_Max = 0.5118f;
        public const float Sim_Lin_Min = -0.5118f;

        public const float Conv_Table_Obs_Zero = -1.103643f;
        public const float Conv_Table_Obs_Max = -0.4970205f; 
        public const float Conv_Table_Obs_Min = -1.5f;
        public static float Z_Correction(float z_pos)
        {
            float cor = ((z_pos - Conv_Table_Obs_Min)/(Conv_Table_Obs_Max - Conv_Table_Obs_Min)) * (Sim_Lin_Max - Sim_Lin_Min) + Sim_Lin_Min;
            return cor;
        }

    }

    public static class Table
    {
        public const int robot_goalX = 1172;
        public const int robot_goalY = 341;
        public const int player_goalX = 0;
        public const int player_goalY = 341;
        public const int goalWidth = 200;
        public const int width = 682;
        public const int length = 1172;
    }

}
