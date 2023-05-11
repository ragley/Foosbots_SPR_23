import math
import time
from FSMConstants import *

async def compute_next_state(currentState, ballX, ballY, rod, stop, ball_hidden, kick_timer, currentDisplacement, currentAngle):
    if stop:
        return "Stop"
    elif abs(ballX - rod["rodX"]) > IDLE_RANGE or ballX == -1:
        return "Idle"
    elif (currentState == "Recover_1" and abs(currentAngle - 0) < MOVEMENT_MARGIN):
        return "Recover_2"
    elif (currentState == "Recover_2" and abs(currentDisplacement - compute_rod_linear(rod, ballY)) > RECOVERY_LINEAR) or (currentState == "Recover_3" and abs(currentAngle - RECOVERY_ANGLE) > MOVEMENT_MARGIN):
        return "Recover_3"
    elif (currentState == "Recover_3" and abs(currentAngle - RECOVERY_ANGLE) < MOVEMENT_MARGIN) or (currentState == "Recover_4" and abs(currentDisplacement - compute_rod_linear(rod, ballY)) > MOVEMENT_MARGIN):
        return "Recover_4"
    elif (time.perf_counter() - kick_timer > KICK_TIMEOUT and kick_timer != 0) or (ballX > GOAL_ROD["rodX"] and rod["rodX"] > GOAL_ROD["rodX"] - 5):
        return "Recover_1"
    elif (currentState in ("Prep","Kick") and OPEN_KICK_RANGE < (rod["rodX"] - ballX) < BLOCK_KICK_RANGE) or (currentState is "Prep" and ball_hidden):
        return "Kick"
    elif OPEN_PREP_RANGE < (rod["rodX"] - ballX) < BLOCK_PREP_RANGE:
        return "Prep"
    elif ballX > rod["rodX"]:
        return "Open"
    else:
        return "Block"
    

async def compute_command(currentState, rod, ballX, ballY, table, currentDisplacement):
    if currentState == "Stop":
        return state_stop()
    elif currentState == "Idle":
        return state_idle(rod)
    elif currentState == "Open":
        return state_open(rod, ballY)
    elif currentState == "Block":
        return state_block(rod, ballX, ballY, table["robot_goalX"], table["robot_goalY"])
    elif currentState == "Prep":
        return state_prep(rod, ballX, ballY, table["player_goalX"], table["player_goalY"])
    elif currentState == "Kick":
        return state_kick(rod, ballX, ballY, table["player_goalX"], table["player_goalY"], currentDisplacement)
    elif currentState == "Recover_1":
        return state_recover_1()
    elif currentState == "Recover_2":
        return state_recover_2(rod, ballY, table["player_goalY"])
    elif currentState == "Recover_3":
        return state_recover_3()
    elif currentState == "Recover_4":
        return state_recover_4(rod, ballY)
    else:
        return [LAST_POSITION, LAST_POSITION]
    

def compute_rod_linear(rod, desiredY):
    maxActuation = rod["maxActuation"]
    playerSpacing = rod["playerSpacing"]
    if desiredY < MIN_PLAYER_OFFSET:
        return 0
    if desiredY > MAX_PLAYER_OFFSET:
        return maxActuation

    if playerSpacing > maxActuation:
        actuation = (desiredY-MIN_PLAYER_OFFSET) % playerSpacing
    else:
        overlap = maxActuation - playerSpacing
        adjusted_actuation = maxActuation - int(overlap / 2)
        player_offset =  int((desiredY-MIN_PLAYER_OFFSET) / adjusted_actuation)
        actuation = ((desiredY-MIN_PLAYER_OFFSET) % adjusted_actuation) + int(overlap * player_offset / 2)

    if actuation > maxActuation:
        return 0
    else:
        return actuation

def state_stop():
    return [LAST_POSITION, LAST_POSITION]

def state_idle(rod):
    maxActuation = rod["maxActuation"]
    return [maxActuation / 2, BLOCK_ANGLE]

def state_open(rod, ballY):
    desiredY = ballY
    return [compute_rod_linear(rod, desiredY), OPEN_ANGLE]

def state_block(rod, ballX, ballY, goalX, goalY):
    if rod["rodX"] < 700 or ballX > 1000:
        desiredY = ballY
    else:
        slope = (goalY - ballY) / (goalX - ballX)
        b = ballY - slope * ballX
        shot_intercept = slope * rod["rodX"] + b
        desiredY = shot_intercept

    return [compute_rod_linear(rod, desiredY), 0]

def state_prep(rod, ballX, ballY, goalX, goalY):
    deltaY = 17.5 * math.degrees(math.sin(math.atan(math.radians( (goalY - ballY) / (goalX - ballX) ))))
    desiredY = ballY + deltaY
    return [compute_rod_linear(rod, desiredY), PREP_ANGLE]

def state_kick(rod, ballX, ballY, goalX, goalY, currentDisplacement):
    deltaY = 17.5 * math.degrees(math.sin(math.atan(math.radians( (goalY - ballY) / (goalX - ballX) ))))
    desiredY = ballY + deltaY
    kick_position = compute_rod_linear(rod, desiredY)  
    if abs(kick_position - currentDisplacement) < PLAYER_LENGTH:
        return [kick_position, KICK_ANGLE]
    else:
        return [kick_position, LAST_POSITION]
    
def state_recover_1():
    return [LAST_POSITION, BLOCK_ANGLE]
    
def state_recover_2(rod, ballY, goalY):
    if ballY < goalY:
        direction = 1
    else:
        direction = -1       
    desiredY = ballY + (direction * RECOVERY_LINEAR)
    return [compute_rod_linear(rod, desiredY), BLOCK_ANGLE]

def state_recover_3():
    return [LAST_POSITION, RECOVERY_ANGLE]

def state_recover_4(rod, ballY):
    return [compute_rod_linear(rod, ballY), LAST_POSITION]

