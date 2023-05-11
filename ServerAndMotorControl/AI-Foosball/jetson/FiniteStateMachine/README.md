Documentation for the Server portion of AI-Foosball Spring 2022 Senior Design Project.

# Finite State Machine

## Purpose

This module is responsible for determining commands for each rod based on the collected data from the camera and motor controllers. This data is received via the server 
and the resulting commands are sent back to the server in the format 

`robot_(rod descriptor)_rod_(displacement/angle)_command`

rod descriptors include (goal, 2, 5, 3) denoting which rod is referenced by the number of players

displacement/angle denotes which axis of the rod is referenced

8 of these commands are generated for each set of data that is collected. The FSM does not care about how old the data in the server is or if it has already computed 
commands on that data previously. The FSM computes faster than vision collects data or the CAN bus sends data so this method ensures that the most recent and accurate 
commands are always available on the server.

## Warning!!!
One must take special care in the design of the FSM or replacement for it. It is highly likely for the rods to either trap the ball between a player and the ground or up
against a wall. This can not be detected or prevented by the motors and they will attempt to torque through the ball and potentially damage the rod mechanism or cause 
gears to skip. Skipped gears require being manually reset and increase the liklihood of further damage to the table and lack of control of the rods. For this reason rods 
should also be zerod whenever they become visually off from their correct position. 

## States
A breif description of each of the FSM states. Each rod operates a copy of this FSM simultaneously allowing the rods to all move independently. 

### Stop
This state is triggered externally when motor controllers are not sending commands, either from an emergency stop being triggered, communication failure, or the game 
being paused. In this state the FSM continually output the previous command leaving the rods in their current location. 

### Idle
When the ball is a certain distance away the rods placed in the center of the table. This stops the rods from responding to values far from the rod and places them
in a central position to react as the ball approaches. 

### Open
This state is triggered when the ball is behind the rod. The players orient themselves parallel with the table to allow shots from behind to pass through 
undisturbed. The players still track the ball to place them in an optimal location after the ball passes underneath. 

### Block
This state is entered when the ball is in front of the rod. The 5 and 3 rod attempt to place a player directly in front of the ball to prevent forward progress. The 
goal and 2 rod place a player between the ball and the center of the goal, priortizing the defense of the goal.

### Prep
When the ball is close enough to the rod, it swings the players back and attempts to position itself offset from the ball slightly in order to kick the ball at an angle to 
approach the opposing goal. 

### Kick 
When the ball is closer to the rod. The rod waits until it has reached the location for kick before swinging the players forward. 

### Recover 
This is a series of four states that attempt to move the ball out of a dangerous positon. Such as underneath the rod after a failed kick or behind the goal rod. These 
positions are very likely for the ball to trapped and damage the mechanisms of the table. The rod first moves itself to a vertical position and then to the side of the
ball closer to the goal. The player than angles back and moves to the position of the ball. This should hit the ball from the side and push it to a better position to 
kick from. 

## FSM Constants

### Network
Variables related to connection with the server
- LOCALHOST: ip address of localhost
- PI_ADDRESS: ip address of the server on the raspberry pi
- PORT: port number of server on the raspberry pi

### State
variables related to the transition and execution of states, all distances in millimeters
- MOVEMENT_MARGIN: how close the rod must be to the desired location to transition in the recover states
- KICK_TIMEOUT: number of seconds before the kick state is forcibly exited
- LAST_POSITION: tells the FSM to resend the previous command
- PLAYER_LENGTH: length of player pad from center point
- NOISE_THRESHOLD: distance differences below this value are ignored to combat input noise
- MIN_VELOCITY_THRESHOLD: velocities below this number are stationary
- OPEN_PREP_RANGE: distance behind a rod where it enters the prep state
- BLOCK_PREP_RANGE: distance in front of a rod where it enters the prep state
- OPEN_KICK_RANGE: distance behind a rod where it enters the kick state
- BLOCK_KICK_RANGE = distance in front of a rod where it enters the kick state
- KICK_ANGLE: how far forward a rod swings when it kick
- PREP_ANGLE: how far back a rod swings when it is prepped
- BLOCK_ANGLE: angle for a blocking rod
- OPEN_ANGLE: angle for an open rod
- SPEED_THRESHOLD: speeds under this value will allow a kick attempt
- MIN_PLAYER_OFFSET: the closest a player can get to the near wall with the bumpers
- MAX_PLAYER_OFFSET: the closest a player can get to the far wall with the bumpers
- IDLE_RANGE: distance between a ball and rod before it idles 
- RECOVERY_LINEAR: distance between ball and player when recovering
- RECOVERY_ANGLE: angle rod takes when in recovery state

### Physical Dimensions
dimensions measured on the table, in millimeters
- GOAL_ROD: dimensions of the goal rod
- TWO_ROD: dimensions of the 2 rod
- FIVE_ROD: dimensions of the 5 rod
- THREE_ROD: dimensions of the 3 rod
- TABLE: dimensions of the table itself

## Files 

### brainFSM.py

connect_to_server(): Given that the server exists on a different computer than the FSM, there is no guarantee that the server will exist when the FSM first starts. This method continually tries to connect to the server until the connection is successful, after a success the function returns the created socket.

compute_intercepts(): computes the y value the ball will cross each rod given its current velocity, returns -1 if the intercept is not in the table. This function does work in theory, however given the inconcistency of the velocity values the results of this function are not very reliable and were not used in the final implementation.

ball_speed(): similar to intercepts, the inconcistency of velocity values resulted in this value not being utilized as origionaly intended

main(): the main function starts by defining the dictionary keys for values needed from the server and keys being sent to the server as well as arrays for current states and kick timeouts. On each loop through the program, data is requested from the server and checked to ensure it was all retreived. If server data is not returned, the rest of the program is not run and the data is requested again. It is important that the server data dictionary is not overwritten as that would compromise future requests to the server. Once running, the location of the ball is checked. If the ball location is not found but the last known location is on the table the last ball location is used and the ball is marked as hidden. Additionally, if the new ball location is not further away than the noise threshold, the last ball position is used to avoid shaking the players with small position adjustments. If a goal was scored or the ball is seen off the table the ball location is marked as not found. A gather operation is performed computing the next state and command for each of the rods. A gather operations schedules each function to be executed asynchronously, retruning an array when all have completed. The output of the gather is then used to update the saved states and fill the dictionary to be sent to the server. A check is hten made to see if any rod entered the kick state. If so, a timer is started. Otherwise the timer is set to False. The compiled commands are finally sent to the server.

### rodFSM.py
this file contains all the functions used to compute the actions of a single rod

compute_next_state(): Given the data from the server returns a value for the next state of a rod. It is important to make sure that the more specific states are checked first as broad conditions will prevent the other states from being reached. Stop and Idle both have straightforward implementations. The recover states have two conditions, one for moving from the previous state and another for remaining in that state. Recovery is entered if the ball is behind the goal rod or the kick timer expires. Kick can be entered once the ball enters a region around the rod or if the rod was prepped and then the ball becomes hidden. The rod assumes it is blocking the ball from the camera and kicks anyway. Prep is entered if the ball is in a wider range from the rod and teh rod is open if the ball is behind it. If no other condition applies the rod should block.

compute_command(): calls and returns the specific function for each state

compute_rod_linear(): given a rod and desired Y location, this function returns the actuation distance to place a player at that location. This function will always put the same player at the same location and does not take into consideration which player is closer if two players can reach a position. If the desired Y is outside of the actuation range, the max or min actuation is returned. There are two scenarios for rods, if overlap exists between players of not. Overlap exists if the max actuation distance of the rod is further than the spacing between players. If no overlap exists, the distance past the players start location is returned by taking the modulus of desired Y  and player spacing. The offset is subtracted from the desired Y as the players start away from the wall due to the bumpers. 
This method does not work on rods with overlap as it assumes the location of the second player at actuation 0 is the same as the first player at max actuation. We start by adjusting the actuation distance to split each of the overlapped regions in half. We then count how many players are between the desired Y and and the 0 position. For example, the 3 rod with a desired location in the middle of the field should use the second player. Player offset will be 1 as there is 1 player player between the second player and the 0 position. The assumed start postion of players is equal to half the offset multiplied by the player offset, this value is then added to the distance calculated for a no overlap rod with the adjusted actuation.

state_stop(): returns last position

state_idle(): returns vertical rods at half actuation

state_open(): returns horizontal rods with player at ball location

state_prep(): deltaY is the additional distance to the side of the ball needed to kick at an angle towards the goal. A player is sent to the ball location plus deltaY with a backwards angle

state_kick(): still outputs the location from the prep state and keeps the prep angle until it reaches the location for the kick. The angle is then set forward

state_recover_1(): sets the rod upright in its current positon

state_recover_2(): places the rod at a set distance on the inner side of the ball

state_recover_3(): rotates the rod back

state_recover_4(): moves the rod back to the ball location

### Other Files
These consist of various other test such as isolating control of linear or rotational movement.
