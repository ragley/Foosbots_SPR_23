#NETWORK 
LOCALHOST = "127.0.0.1"
PI_ADDRESS = "192.168.0.1"
PORT = 5000

#STATE
MOVEMENT_MARGIN = 2
KICK_TIMEOUT = 1
LAST_POSITION = -1
PLAYER_LENGTH = 2
NOISE_THRESHOLD = 3
MIN_VELOCITY_THRESHOLD = 300  
OPEN_PREP_RANGE = -30
BLOCK_PREP_RANGE = 100
OPEN_KICK_RANGE = -20
BLOCK_KICK_RANGE = 60
KICK_ANGLE = 55
PREP_ANGLE = -30
BLOCK_ANGLE = 0
OPEN_ANGLE = -90
SPEED_THRESHOLD = 3000
MIN_PLAYER_OFFSET = 40
MAX_PLAYER_OFFSET = 640
IDLE_RANGE = 600
RECOVERY_LINEAR = 80
RECOVERY_ANGLE = -57

#PHYSICAL DIMENSIONS
GOAL_ROD = {"maxActuation":228, "playerSpacing":182, "rodX":1125, "numPlayers":3}
TWO_ROD = {"maxActuation":356, "playerSpacing":237, "rodX":975, "numPlayers":2}
FIVE_ROD = {"maxActuation":115, "playerSpacing":120, "rodX":675, "numPlayers":5}
THREE_ROD = {"maxActuation":181, "playerSpacing":207, "rodX":375, "numPlayers":3}
TABLE = {"robot_goalX":1200, "robot_goalY":350, "player_goalX":0, "player_goalY":350, "goalWidth":200, "width":685, "length":1200}
