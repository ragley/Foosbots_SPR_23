from FSMConstants import *

#TODO: check width and length values 
def IRL_2_U_X(irl_X):
    return U_TABLE_LENGTH_MAX - ((irl_X / TABLE["length"]) * U_TABLE_LENGTH_MAX)


def IRL_2_U_Z(irl_Y):
    return ((irl_Y / TABLE["width"]) * (U_TABLE_WIDTH_MAX - U_TABLE_WIDTH_MIN)) + U_TABLE_WIDTH_MIN


def NN_OP_2_IRL_ROT(nn_rot):
    return (nn_rot * 180)


def NN_OP_2_IRL_LIN(rod, nn_lin):
    maxActuation = rod["maxActuation"]
    irl_Lin = ((nn_lin - (-1)) / (1 - (-1))) * maxActuation
    return irl_Lin
