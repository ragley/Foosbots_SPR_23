using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Converters
{
    public static float IRL_2_U_X(float irl_X)
    {
        return Constants.U_TABLE_LENGTH_MAX - ((irl_X / Constants.Table.length) * Constants.U_TABLE_LENGTH_MAX);
    }

    public static float IRL_2_U_Z(float irl_Y)
    {
        return ((irl_Y / Constants.Table.width) * (Constants.U_TABLE_WIDTH_MAX - Constants.U_TABLE_WIDTH_MIN)) + Constants.U_TABLE_WIDTH_MIN;
    }

    public static float NN_OP_2_IRL_ROT(float nn_rot)
    {
        return (nn_rot * 100);
    }

    public static float NN_OP_2_IRL_LIN(int maxActuation, float nn_lin)
    {
 
        float irl_Lin = ((nn_lin - (-1)) / (1 - (-1))) * maxActuation;
        return irl_Lin;
    }

    public static float ComputeRodLinear(int maxActuation, int playerSpacing, float desiredY)
    {
        float actuation = desiredY;

        if (desiredY < Constants.MIN_PLAYER_OFFSET)
        {
            return 0;
        }
        if (desiredY > Constants.MAX_PLAYER_OFFSET)
        {
            return maxActuation;
        }

        if (actuation > maxActuation)
        {
            return 0;
        }
        else
        {
            return actuation;
        }
    }
}
