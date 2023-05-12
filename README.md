# __Oklahoma State AI Foosball__
This project leverages the Unity game engine and ML Agents, an open source package for Unity, to train a neural network via reinforcement learning to play the game of foosball on a physical foosball table.

A video overview is availible [here](https://www.youtube.com/watch?v=JQMWti5Pj_U).
<br></br>

## __Getting Started__
First, you will need to install Unity 2021.3.11 which can be found at the following link: <https://Unity.com/releases/editor/archive>

In order to utilize the precise version of python required, it is recommended that all work be done within a virtual environment. An easy guide for setting up a virtual environment can be found here: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Virtual-Environment.md>

Once your virtual environment is activated follow these steps to install all necessary technologies:

  1.) Install Python 3.7.7 which can be found at the following link: <https://www.python.org/downloads/release/python-377/>
  
  2.) Install PyTorch by running the following command from the command line:
  
      pip install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
    
  3.) Install ML Agents' Python package by running the following command from the command line:
  
      python -m pip install mlagents==0.28.0
  4.) Install importlib-metadata Python Package
      
      pip install importlib-metadata==4.4 
  5.) Download the Unity Assets Hosted in the MS Teams files. They are too large to be hosted here unfortunately, and git LFS does not function for public repositories.

<br></br>

## __Important Unity Editor Components__

### __Important Scripts__
<details>
  <summary>Click to Expand</summary> 

| Name        | For        | Description | Location    | 
| ----------- |----------- | ----------- | ----------- |
| foosball_config.yaml | Configuration | Configuration file for neural network | `Assets\config` |
| SelfPlayAgentJoint.cs | Unity | Gives brain a body for acting on the simulated table. Defines foosball objects, collects observations, sets rewards/penalties, and outputs decisions | `Assets\Scripts\Agents` |
| Dummy Agent.cs | Unity/IRL Table | Acts as a dummy agent, essentially giving brain a body to act on the physical table. | `Assets` |
| DummyInference.cs | Unity/IRL Table | Inference file that takes in IRL table positions from server, runs that through the trained NN, and outputs decisions. Use to run NN on physical table. | `Assets\Scripts` |
| TableEnvHandler.cs | Unity | Handles table controlled variables (scoring, reset, gamemode, etc.) Allows multiple tables to run simultaneously and independently. Syncs variables between agents. | `Assets\Scripts` |
| Ball.cs | Unity | Defines the ball and ball collision detection events. | `Assets\Scripts` |
| cameraController.cs | Unity | Allows user to switch and move camera. `C` to switch, `Q` and `E` move in play mode or build. | `Assets\Scripts` |
| Constants.cs | Unity | Contains all constant values for NN. Separated by rod | `Assets\Scripts` |
| converters.cs | Unity | Converts Table <--> Unity linear and rotational positions | `Assets\Scripts` |
| GoalScored.cs | Unity | Called when ball goes through goal. Determines who scores, resets ball | `Assets\Scripts` |
| Message.cs | Server | Server message that contains IRL table position | `Assets\Scripts` |
| PlayerColor.cs | Unity | Defines team colors: Blue, Red, or None | `Assets\Scripts` |
| ball_tracking_limeGreen.py | IRL Table | Outputs ball position and velocity to server. Runs separate from Unity | `Assets\Scripts\Python` |
| brainFSM.py | IRL Table | FSM for ye ol' table. Reference only. Not used in Neural Network. | `Assets\Scripts\Python` |
| FSMConstants.py | IRL Table | Reference only. Not used in Neural Network. | `Assets\Scripts\Python` |

</details>
<br></br>

### __Important Environment Objects__

 - `CanvasCam2`: This object holds the visible elements of the UI panel.
 - `TableEnvHandler`: This object holds the the `TableEnvHandler.cs` script. Its public variables can be viewed and edited in the component of this object
 - `Table`: Holds the 3D components and colliders of the table itself. 
 - `Team.Blue` and `Team.Red`: This is the object containing all child objects and components of the Agent. The `Behavior Parameters`, `Decision Requester`, and `SelfPlayAgentJoint.cs` are all attached to this object.
   - `Team.<PlayerColor>.<Rod>`: These are the objects for each player rod. They each contain the rod Rigidbody, Configurable Joint, and children player objects with their colliders.
   - `Team.<PlayerColor>.Goal`: This is the goal object, and includes the trigger collider for goal detection.
<br></br>
### __Behvaior Parameters__
This component determines how an agent makes decisions and  has several important sub-components `NEEDS PIC UPDATE`

![foosball_behavior_parameters](https://user-images.githubusercontent.com/35296087/206737538-0e228616-4cff-4555-ba08-375b74c02f06.png)

`Behavior Name:` This field must match the behavior named specified in the `.yaml` file referenced during training

`Vector Observation Space:` The number of elements in the `CollectObservations()` method, we have 46 in our script.

`Actions:` The size of the action space, for this project there are 8 continuous action outputs (2 per rod, translation, rotation) and 0 discrete actions.

`Model`: Allows for the option of loading a brain for inference. Put the brains in `Assets/Brains/` to have them show up. (Make sure to name them someting easily identifiable!)

`Behavior Type:` What type of behavior the AI has. 
- `Default` Will either run inference with the selected brain if not training, or if `mlagents-learn` is active it will train.
- `Inference Only`: Like it says, will always run inference on selected device, useful if training one team only.
- `Heuristic Only`: Like it says, useful for controlling rods yourself and seeing actuation behaviors.


`Team ID:` Which team each agent is on, it is important for the self play training that each team have a different ID.
<br></br>

### __Decision Requester__
This component determines how often the agent makes decisions. The `OnActionsRecieved()` function gets called every `Decision Period` steps. The agent makes decisions and outputs commands at this step. 

For Example:
- If the environment goes through 300 steps with the decision requester is set to 12, the Agent will make decisions every 12 steps, for a total of 25 decisions being made.   

![foosball_decison_requester](https://user-images.githubusercontent.com/35296087/206738764-6a268c05-15dd-489e-bd67-06885dccc74e.png)


>*__NOTE:__*
>
>Ensure *"Take Actions Between" checkbox is unchecked and that the `Decision Period > 1`. We found that when allowed to make decisions at every step (Decision Period = 1), the agent would get stuck moving its rods only in one direction. However, when it was slightly throttled it had full range of motion. When training in the simulation, a value of 12 produces a better simulated result, while a value of 20 produced a better result on the physical table. This is likely due to the AI learning to compensate for delays in input which it might experience on the physical table. When running on the physical table the `Decision Period = 1`.

<br></br>

### __Important Agent Script Details__
There are several agent scripts included in the project from the various phases of testing and developing the project. The one that is currently being used and is most up to date for the simulation is called `SelfPlayAgentJoint.cs`. Public variables should be set in the `TableEnvHandler.cs` to ensure settings are the same for both agents. Important functions are explained in the *Click to Expand* section under each script. The simulation utilizes the following scripts: 

<br></br>
__`SelfPlayAgentJoint.cs`__
<details>
  <summary>Click to Expand</summary>

### __`OnEpisodeBegin()` - Void__
This method is called at the start of every training episode. Currently the only thing here is a counter to display how many times the `OnActionsRecieved()` function is called.
<br></br>

### __`CollectObservations()` - Void__
This method is utilized to obtain observations for the neural network and is called every `FixedUpdate()`. Every observation must be a single number. So, if you wish to pass something like the (x,z) coordinates of the ball's location, this would be two separate observations. Observations undergo Domain Randomization, with upper and lower multipliers defined as public variables, and are put into an array, which is then jooped over and applied to the `AddSensor()` function. 

**IMPORTANT:** Observations for the agent should ***ALWAYS*** be from the perspective of the **ALLY** goal. Functions have been created to do this, they are `getGoalRelPos()` and `getGoalRelDir()`. This keeps the observations symmetric for both agents. If this convention is not followed, it's likely you'll end up with an agent brain that plays properly on one team but not the same on the other. This is **BAD**, as the physical table AI Teams are on different sides.
<br></br>

### __`OnActionReceived()` - Void__
This method is the main driver function of the agent in charge of both producing actions and producing rewards, this function is called every interval set in `Decision Requester`. The `OnActionsRecieved()` function is called every `DecisionInterval` `FixedUpdate()` steps. This project makes use of a continuous action space where all outputs are naturally in the range (-1,1), but we manually clip them to guarantee that they stay in that range, per recommendation of the ML Agents team. Currently the actions of the agent are in the form of "Desired Position" or where the agent wants to be in terms of its minimum and maximum actuation values for the specific action. 


To actuate the rods themselves based on the outputs from the neural network, each rod has two actuation values that move the rod. 
- __For Linear:__
  
      allyAttackRod.AddForce(Vector3.Scale(inputRandomization, getRodVelLinear(0, allyAttackRod.transform.localPosition, controlAttackForce.z)), ForceMode.VelocityChange);

  This function call adds force to the rod (Unity's method of moving Rigidbodies continuously with physics) in a very specific manner that is indicitave of motor control. The force is applied with Unity's `ForceMode.VelocityChange` method, which ignores object mass and directly sets the velocity, but in a continous manner. The values are determined by the `getRodVelLinear()` function and the outputs from the neural network. Additionally the Agent's actuation vector derived from it's output is randomized by the `inputRandomization` variable. This aids in domain randomization, helping the AI not to rely on fully reliable outcomes to its outputs.



- __For Rotation:__
      
      allyAttackRod.AddTorque(Vector3.Scale(inputRandomization, getRodVelRot(allyAttackRod.transform.localRotation.z, controlAttackTorque.z)), ForceMode.VelocityChange);
  The same methodology is applied to rotation, but instead uses `getRodVelRot()`.

>*DO NOT EVER set the velocity with `rbody.velocity`. This will result in unreliable physics. If you are needing to change velocity on a rigidbody for some reason use `ForceMode.VelocityChange`.* 
<!-- (◐ω◑ ) -->

<br></br>
The reward structure is designed to work with curriculum based training or Self Play, depending on settings defined in the Unity build and training configuration `.yaml` file. Rewards are designed to be symmetric in their application, meaning you should be able to train either red or blue team individually (or both in terms of Self Play), without modifying the reward code. 

Rewards in this section can be tricky. The way the simulation is currently set up, ONLY rewards that affect a single agent should be placed in this section. If any rewards here are defined based on the outcome of the other agent's actions, a race condition will develop.

For Example:
- Scoring
  - Should NOT be in `OnActionsRecieved()`.
  - Scoring, by definition, should end the episode. If the episode end is called here, it will skip the agent who doesn't register the score first, and thus must be handled in the `TableEnvHandler.cs`
- Shot Rewards
  - Should be in the Agent Script in `OnActionsRecieved()`
  - A the outcome or evaluation of a shot an agent makes has no relation to the other agent, and it's direct outcome will not affect the opponent agent.
<br></br>

### __``ShotReward()`` - Float__
This method serves to evaluate the reward assigned to a shot made by the agent. Currently it calculates two vectors, all values are in relative space of the Agent's ally goal to ensure the both sides are symmetric from the perspective of the agent: 
- `deltaAllyGoal`: the vector between the ball's position and the ally goal
- `deltaEnemyGoal`: the vector between the ball's position and the enemy goal

These represent the "worst" and "best" possible directions for the ball to travel, respectively. The dot product of the unit vector for each delta, and the current ball's velocity (in relative space of the ally goal) is calculated. The following variables determine how the reward defined from the resulting `shotValue` is applied: 

Boolean variables within the Agent script change how this value is applied. These are:
- `useNegShotPenalty`
  - If `true`: Allows the value of the shot reward to be negative.
  - This picks the larger of the two, and rewards/penalizes based on the chosen value.
  - If `false`: The `shotValue` is floored at 0. 
- `usePosessionEval`
  - If `true`: Only apply the shot evaluation when the agent was the last one to kick the ball (i.e. If Red kicks the ball, the shot evaluation is only applied every step until blue hits the ball, then it applies to blue team)
  - If `false`: The ball's trajectory is continuously evaluated and both agents are rewarded respectively.
- `useSingleShotReward`
  - If `true`: Only apply the shot evaluation only when agent makes contact with the ball. 

The resultant float value is multiplied by a user-defined `shotRewardMultiplier` and returned. I've found with `useNegShotPenalty=true`, `usePosessionEval=false`, and `useSingleShotReward=false`, results in a purely symmetric reward system: if red gets `+X` blue gets exactly `-X` for the shot reward at any given time.<br></br>

### __`SpinPenalty()` - Float__
This function is to prevent the rods from endlessly spinning as fast as possible. Previously the AI would find this is the best solution to getting the ball in the goal as fast as possible.  <!--  its right, but that not how you play foosball -->

*This function currently needs further testing and balancing and/or is obsolete. Currently Rods are unable to fully spin more than 354 degrees (-177, 177)*
<br></br>

### __`SummaryStr(string endType)` - String__
This is a function for debugging to print out the total reward recieved by the agent at the end of each episode, as well as individual reward breakdowns. It can be called from a function `TableEnvHandler.cs` to display breakdowns of both agents.
<br></br>

### __`desPosition(int rod, float inputDesiredPos, string space)` - Float__
This function converts the desired position output by the neural network `(-1, 1)` to a value that corresponds to a linear position for each rod. The `space` parameter can be either `global` or `local` depending on the context this function is used in, and will return values in that space.
<br></br>

### __`getRodVelLinear(int rod, Vector3 curPos, float inputDesiredPos)` - Vector3__
This is a very important function as it's responsible for actuating the rods in a similar manner to the methods used by the physical table. Since the physical table is actuated by stepper motors, operating on desired position commands, the Unity simulation is required to actuate in a similar manner. The difficulty with this is maintaining a continuous physics simulation that allows for consistent collisions. The rods could be easily actuated with the `Transform.position` and `Transform.rotation` commands within Unity but these ignore the physics system. Taking inspiration from the [paper published by KIcker](https://www.researchgate.net/publication/341204434_KIcker_An_Industrial_Drive_and_Control_Foosball_System_automated_with_Deep_Reinforcement_Learning), the decision was made to utilize target velocities.

The function takes in the current `Vector3` Position the selected rod, as well as calls `desPosition()` to get the neural network's desired `z` position. Both these values are converted to relative space to the ally goal. It then calculates the velocity required to get the rod from the current position, to the desired position in one timestep, capped at a specific velocity, ideally set to a value correspondig to the table's max actuation speed.
<br></br>

### __`getRodVelRot(float curPos, float inputDesiredPos)` - Vector3__
This function works similarly to `getRodVelLiner()` as it takes the desired position from the neural network and normalizes the value to `(-180, 180)`, then calculates the velocity required to reach the desired position in one timestep.
<br></br>

### __`getGoalRelPos(Vector3 obj)` - Vector3__
This function returns a `Vector3` object corresponding to the given position in relative space of the agent's ally goal.
<br></br>

### __`getGoalRelDir(Vector3 obj)` - Vector3__
This function returns a `Vector3` object corresponding to the given direction in relative space of the agent's ally goal. Useful for direction vectors like velocity.
<br></br>

### __`getHitAngularVelocity(Collision collisionData)` - Float__
Returns a float corresponding to the angular velocity of the rod that makes collision with the ball.
<br></br>

### __`Reset()` - Void__
This function resets the agent's position to the default values, i.e. centered with a rotation of 0 (feet straight down). It is currently called from the `Reset()` function in `TableEnvHandler.cs`.
<br></br>

### __`Heuristic()` - Void__
This method is used for manual testing. When "heuristic only" is selected within the "behavior parameters" section in the Unity editor, the network will not take actions on its own and instead the user can use the arrow keys to move the rods.
<br></br>
</details>

<br></br>
__`TableEnvHandler.cs`__
<details>
  <summary>Click to Expand</summary>

### __`Start()` - Void__
This function is the initial setup function within Unity. It is called only once when the program is in play mode. Currently it:
- Gets the ball `Rigidbody` component
- Sets the `autoKickStrength`
- Sets the value of `timeStepPenalty` (`1 / (MaxEnvSteps / decInterval)`)
- Resets the scene so everything is in the default starting position
- Sets the agent variables to match the environment handler to ensure they are the same.
- Gives the ball its initial kick
<br></br>

### __`FixedUpdate()` - Void__
This is a default Unity function that gets called every incremental update of the Unity engine. Anything that affects both agents simultaneously needs to go here so race conditions between multiple agents don't develop. Currently this function handles curriculum variable changes with the ```lesson``` variable retrieved from the config `.yaml` file through:

    lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("reward_switch", 0.0f);

Currently these lessons change some reward values, and game type. Game types include `touch_ball` and `reg_play`. 
- `touch_ball`: Ends the episode as soon as blue touches the ball and is used to train the brain to understand where its player feet are and to associate touching the ball with them. Use to train behaviors only with one actively learning agent (default: `Blue`)
- `reg_play`: General foosball game behavior, used to train more complex and longer-term strategy and behavior such as shots and scoring. 

In addition to game-types the UI can be updated from here. For adding new components to the UI, make changes to `UIManager.cs`
<br></br>

### __`ResetScene()` - Void__
This function is responsible for resetting the scene so a new episode can begin properly. It
- Calls the `Reset()` function of each agent
- Resets the ball postion. 
  - Leftover code is availible commented out for resetting the ball only on the midfield line randomly as opposed to the whole table if it's desired to be used.
- Randomizes the angular drag of the ball by +/- 10% so the AI doesn't rely on ball physics being identical every time.
- Sets `hitAngularVelocity` and `hitVelocityReward` back to `0`
- Resets episode timer
- Sets the agent variables to be the same as they are in `TableEnvHandler.cs` in case they have changed somehow
- Increments episode count in UI
<br></br>

### __`EndSummary()` - Void__
Debug statement that gets the `SummaryStr()` from each agent and prints it to the debug console. Useful for ensuring rewards are getting implemented properly. 

>*__NOTE:__*
>
>*Disable calls to this before building as it will slow down the simulation, especially running lots of tables simultaneously.* 

<br></br>

### __`SetAgentVars()` - Void__
This function syncs variables of the `TableEnvHandler.cs` to the agents. Ideally, you will set all variables in the `TableEnvHandler.cs` and they will be propogated to the Agents properly. If you add more variables to the agent that affect the behavior, add a variable here to sync them properly.
<br></br>
</details>
<br></br>

### __Importand Scripts for Inference__
In order to run inference on the physical table a moderately complex approach had to be taken due to the need of the neural network .onnx brain.  While this is a "usable" neural network brain type generally, since it was created with Unity's ML-Agents, it requires the Barracuda library to be properly parsed and correct outputs generated. Thus the decision was made to run inference within the Unity engine to avoid incorrect outputs. 

To do this I modified the original Python inference script I created to run entirely within Unity in C#. Essentially, we trick the brain into still thinking it is playing within the Unity simulation by feeding it the appropriate data as inputs from the physical table. This is why the inference script as well as the inference agent are called `DummyInference.cs` and `DummyAgent.cs` respectively.
<br></br>

`DummyInference.cs`
<details>
  <summary>Click to Expand</summary>

### __`commands` - Dictionary<string, object>__
Contains names and values of commands that are sent to the server.

### __`server_data` - Dictionary<string, object>__
Contains names and values of data retrieved from the server.

### __`ConnectToServer()` - Socket__
Takes in the `host` ip address and `port` as arguments, and attempts to connect to the Raspberry Pi server. 

>*NOTE:*
>
>*Unity will freeze if it cannot connect to the server. Ensure the server is already running before pressing play in Unity or executing the Unity build.*

### __`Start()` - Void__
Gets the Rigidbody component for the visuals and calls `ConnectToServer()` 

### __`FixedUpdate()` - Void__
This function is the main loop for running inference on the physical table and thus has many components, expanded upon below.
- Rotates the cube visualization shown during play.
- Defines, recieves, and parses data `message` from the server.
- Converts all server data into usable data types
- Checks for a goal condition based on server information, and pauses inference for 6 seconds, ideally to allow the rods to reset.
- Assigns data from the server to the AI's observations. 
  - Ball
    - Gets ball X and Z positions and velocity
  - Player Rods
    - Gets rod position and rotation, as well positions of each player piece
  - Goals
    - Gets goal X and Z positons 
  >*NOTE:*
  >
  > *The data from the server (except ball tracking) is taken from different coordinate spaces than the simulation. All observations in the simulation are taken from the perspective of the ally goal as the origin. Observations for the player rods on the physical table are taken with the "zero" side wall being the Z origin, and the human goal wall as the X origin. Additionally there are correction factors used to ensure the values are as close as possible to the values experienced by the brain within the simulation. The functions that handle these are in `Converters.cs` and `Constants.cs`.* 
- Takes the neural network outputs and translates them to actuation commands for the physical table.
- Sends the commands to the server.

### __Output Commands__
This array is the output decisions of the NN. Max actuation, player spacing, and the conversion required for Unity -> IRL are all accounted for in these outputs. The domains of these values are in relation to locations on the table. 

| Index  | Object                      | Command             |
| ------ | --------------------------- |-------------------- |
| 0      | allyGoalkeeper (goal-Rod)   | Linear movement     |
| 1      | allyGoalkeeper (goal-Rod)   | Rotational movement |
|        |                             |                     |
| 2      | allyDefence (2-Rod)         | Linear movement     |
| 3      | allyDefence (2-Rod)         | Rotational movement |
|        |                             |                     |
| 4      | allyMidfield (5-Rod)        | Linear movement     |
| 5      | allyMidfield (5-Rod)        | Rotational movement |
|        |                             |                     |
| 6      | allyAttack (3-Rod)          | Linear movement     |
| 7      | allyAttack (3-Rod)          | Rotational movement |


</details>
<br></br>


`DummyAgent.cs`

<details>
  <summary>Click to Expand</summary>

The `DummyAgent` is very similar to the `SelfPlayAgentJoint`. It functions generally the same way, but retrieves observations from the server as its inputs. 

>*NOTE:*
>
> *With the neural network running within unity, it is theoretically possible to assign reward structures here and train the AI on the physical table. With the addition of more encoder hardware and additional code, it would also be possible to implement behavioral cloning to attempt to mimic human behavior on the table. 


### __`Start()` - Void__
Initializes score from values retrieved from `DummyInference.cs`.

### __`OnEpisodeBegin()` - Void__
Initializes score from values retrieved from `DummyInference.cs`.

### __`CollectObservations()` - Void__
This function retrieves all input values consolidated in `DummyInference.cs` and adds them to a vector of observations. The order is very important and must be identical to what the AI recieves in the simulation, or improper output will be generated from the brain.

### __Input Observations__
Fourty-six obervations are assigned in the agent script, and then passed along to the Inference file. 
These inputs can be viewed in Unity in the *Dummy Inference (Script)* section within the  **Inspector** tab. 


  <details>
    <summary>Click to Expand Table</summary>

| Index | Object                    | Obervation    |
| ------| ------------------------- |-------------  |
| 0     | Ball                      | *x* position  |
| 1     | Ball                      | *z* position  |
| 2     | Ball                      | *x* velocity  |
| 3     | Ball                      | *z* velocity  |
|       |                           |               |
| 4     | allyAttack rod            | *x* position  |
| 5     | allyAttack rod            | *z* position  |
| 6     | allyAttack rod            | *z* rotation  |
| 7     | allyAttack Player 0       | *x* position  |
| 8     | allyAttack Player 0       | *z* position  |
| 9     | allyAttack Player 1       | *x* position  |
| 10    | allyAttack Player 1       | *z* position  |
| 11    | allyAttack Player 2       | *x* position  |
| 12    | allyAttack Player 2       | *z* position  | 
|       |                           |               |
| 13    | allyMidfield rod          | *x* position  |
| 14    | allyMidfield rod          | *z* position  |
| 15    | allyMidfield rod          | *z* rotation  |
| 16    | allyMidfield Player 0     | *x* position  |
| 17    | allyMidfield Player 0     | *z* position  |
| 18    | allyMidfield Player 1     | *x* position  |
| 19    | allyMidfield Player 1     | *z* position  |
| 20    | allyMidfield Player 2     | *x* position  |
| 21    | allyMidfield Player 2     | *z* position  |  
| 22    | allyMidfield Player 3     | *x* position  |
| 23    | allyMidfield Player 3     | *z* position  |
| 24    | allyMidfield Player 4     | *x* position  |
| 25    | allyMidfield Player 4     | *z* position  |
|       |                           |               |
| 26    | allyDefence rod           | *x* position  |
| 27    | allyDefence rod           | *z* position  |
| 28    | allyDefence rod           | *z* rotation  |
| 29    | allyDefence Player 0      | *x* position  |
| 30    | allyDefence Player 0      | *z* position  |
| 31    | allyDefence Player 1      | *x* position  |
| 32    | allyDefence Player 1      | *z* position  |
|       |                           |               |
| 33    | allyGoalkeeper rod        | *x* position  |
| 34    | allyGoalkeeper rod        | *z* position  |
| 35    | allyGoalkeeper rod        | *z* rotation  |
| 36    | allyGoalkeeper Player 0   | *x* position  |
| 37    | allyGoalkeeper Player 0   | *z* position  |
| 38    | allyGoalkeeper Player 1   | *x* position  |
| 39    | allyGoalkeeper Player 1   | *z* position  |  
| 40    | allyGoalkeeper Player 2   | *x* position  |
| 41    | allyGoalkeeper Player 2   | *z* position  |
|       |                           |               |
| 42    | allyGoal                  | *x* position  |
| 43    | allyGoal                  | *z* position  |
| 44    | enemyGoal                 | *x* position  |
| 45    | enemyGoal                 | *z* position  |
</details>

### __`OnActionsReceived()` - Void__
Similar to the `SelfPlayAgentJoint.cs` script, this function gets the output from the neural network as an array of actions, that is subsequently parsed by the `DummyAgent.cs` script to be sent to the server as movement commands. Additionally, there is an example reward structure included which rewards/penalizes the AI for scoring on the physical table.

### __Neural Network Decisions__
This array of 8 objects is defined within the `OnActionReceived()`, and passed to Inference. Outputs from the neural network are continuous values on the domain of ```[-1, 1]```.  The order of these is the same as they are called within the simulation.

| Index  | Object                      | Action    |
| ------ | --------------------------- |---------- |
| 0      | allyAttack (3-Rod)          | Linear    |
| 1      | allyAttack (3-Rod)          | Rotation  |
|        |                             |           |
| 2      | allyMidfield (5-Rod)        | Linear    |
| 3      | allyMidfield (5-Rod)        | Rotation  |
|        |                             |           |
| 4      | allyDefence (2-Rod)         | Linear    |
| 5      | allyDefence (2-Rod)         | Rotation  |
|        |                             |           |
| 6      | allyGoalkeeper (goal-Rod)   | Linear    |
| 7      | allyGoalkeeper (goal-Rod)   | Rotation  |

### __`Heuristic()` - Void__
The heuristic method allows for manual input from the arrow keys on the keyboard to act as outputs from the AI.


</details>
<br></br> 

## __Training a Brain for Use on the Physical Table__
### __Preperation__
To train a brain for use on the physical table first open the ```Final_BallTouchTrainer.Unity``` scene (availiable on Teams if you're a future project member, or you can send a request to me on discord at Ragley#1700 and I will send you the file. They are too large to put here and git-lfs has trouble with collaborative repos.)

If developing on VSCode on the main computer, make and save your training changes to the scripts you've modified according to the guidelines above. The main ones affecting the agent are the ```SelfPlayAgentJoint.cs``` and ```TableEnvHandler.cs```. 

>*__NOTE:__*
>
>*I would highly recommend testing the changes made in play mode with both agents running in inference to see if your new rewards/etc. are applying properly. Don't fall into the trap of making your changes and immediately jumping into training, only to find out your rewards were not applying properly. There are Unity debug log print statements within ```TableEnvHandler.cs``` that give some general reward breakdowns for each agent. These can be used to ensure rewards are being implemented properly.*

<br></br>

### __Training the Network__

In order to train the network, open the project in Unity. Duplicate as many of the `Foosball_V8_Meters` tables as you want, dragging them into position on the negative x-axis, ensuring they are not overlapping. 
  
  >*__NOTE:__*
  >
  >*Tables can be placed in a regtangular-grid formation, but this reduces performance significantly, so is not recommended if wanting to visually monitor progress.*

Once this is complete, ensure `Product Name` under `Project Settings > Player > Product Name` is: 
    
    Foosbots_MultiRod_5

Then create a build within Unity of the `Final_BallTouchTrainer.Unity` scene, saving it to `<BuildFolder>`. 

Next open a PowerShell terminal with your virtual environment active and navigate to the directory containing the Unity project, and then run the following command:

    mlagents-learn config/<config_name>.yaml --run-id=<TITLE_OF_TRAINING_RUN> --env=<BuildFolder>/Foosbots_MultiRod_5.exe --time-scale=1 --num-envs=<num>
    
Replace with whatever you wish to call that particular session of training. 

`<BuildFolder>` is the directory of the latest built .exe from Unity. 

`time-scale` should always be set to 1. It is tempting to increase this to reduce training time, however evidence shows higher time scale will affect the performance of the resulting brain negatively.

The `--env` parameter can be dropped if testing inside the editor, however the editor noticably reduces performance and is not recommended unless debugging.

To utilize a build that has multiple instances, the best one I've found for using on the main computer (RTX4090, 7950X) with a build of 128 concurrent tables is with:
- `num-envs`: Between `6` and `8`. 8x128 = 1024 concurrent tables

To edit the hyperparameters of the network, open the chosen (or new) `.yaml` file in `/config/` and make any necessary changes, then save the file. When running `mlagents-learn` replace `config_name` with your configuration file. The current config files are: 
- `foosball_config_curr.yaml` for single agent training with a curriculum
- `foosball_config_currSP.yaml` for self play training

> More information about `mlagents-learn` parameters and configuration hyperparameters can be found [here](https://github.com/gzrjzcx/ML-agents/blob/master/docs/Training-ML-Agents.md) and [here](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md) respectively.

<br></br>
### __Accessing and Evaluating Results__
In order to view tensorboard graphs to evaluate training, activate the virtual environment and navigate to the directory containing the Unity project then run the following command:

    tensorboard --logdir results --port 6006
  
Then, navigate to

    localhost:6006

There are 7 statistics used primarily to evaluate the network

- __Cumulative Reward__
  - This measures the mean cumulative reward over all agents and should increase through training
- __Policy Loss__
  - This correlates to how much the policy is changing throughout training and should decrease over time
- __Value Loss__
  - This correlates to how well the model can predict the value of each state and should initially increase but ultimately stabilize
- __Policy Entropy__
  - This correlates to how random the model's decisions are and should decrease over the course of training
- __Policy Learning Rate__
  - This correlates to how large a step the algorithm is taking as it searches for an optimal policy and should decrease over time
- __Policy Value Estimate__
  - This measures the mean estimated value for all states visited by the agent and should be expected to increase over time
- __ELO__
  - This measures the relative skill between two players and in a proper training run this should steadily increase

> *__NOTE:__*
> - *"test_documentation.txt" contains the various hyperparameters used on each of the included training runs.*
> - *Sometimes it takes a while to display new values especially with long training runs saved, these add up to several Gb of data. Just keep refreshing or go grab a coffee, it can take 5-10 minutes to display sometimes.*

More information about the use of tensorboard and evaluation of these statisitics can be found at [here](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Tensorboard.md).
<br></br>

## __Deployment on the Physical Table__
The following steps outline how to deploy a trained brain onto the physical table for testing. 
### __Unity Setup for Inference__
Unity requires configuration for running inference on the physical table, since the brain is running through Unity to get the proper outputs from the neural network.

1) First, load the the scene `FINAL_InferenceDUMMY` in the Unity editor.
2) Select the `Cube` object from the heirarchy, this contains the `DummyAgent.cs` and `DummyInference.cs` scripts as well as all components related to Agent functions.
3) Make a copy of the latest trained brain (`.onnx` file) from the `results/<RUN_ID>` folder, and rename it something reasonable. 
4) Copy the brain to `Assets/Brains/` in order for it to show up in the Unity Editor as an availiable object. 
5) Select your desired `Behavior Type`, likely this will be `Inference Only` if you are wanting to test a brain.

Unity should essentially be set up at this point to start running inference on the physical table. After you do these steps, ensure the table is turned on with the server running, then start the ball tracking program to ensure the server has all necessary data for the AI. 


### __Running Inference__
There are now two options availible for testing your brain:

1) Run the brain through the Unity Editor
   - This is likely the best option if you are testing a new brain for a short time period. It allows for easily hot swapping brains, changing behavior type, adjusting parameters, etc.
   - To do this all you need to do once the server is running is press `Play` in the Unity Editor.
2) Create a build for the Inference.
   - This is the best option for running inference on the table with a brain usable for long term, and for a more "user-friendly" operation experience, compared to a development oriented one. 
   - This does __not__ allow you to change brains, settings, or any other parameters during play.
   - This does however, run faster than running it through the editor, as well as makes it easier to start up the whole system.
   - To do this, create a build as previously mentioned in the Training The Network section. However, it is important to make the following changes if you would like to use the startup script. (Otherwise you can start the programs manually.)
     - The `Product Name` __must__ be:
          
            Foosbots_Inference_Engine
     - The build folder __must__ be `InfBuild` in the root directory.

Now that your brain is running in inference, touch the OSU logo at the top of the Raspberry Pi screen, which opens the debug menu. Ensure all data is being displayed (Ball tracking, rod positions, rod commands, etc.). If everything is there, hit `Play` then any difficulty. Your brain should now be actuating the computer rods on the physical table. To pause play, simply hit `Pause Game`. To unpause, hit `Resume Game` (__Not Play__, it will not work).








## __Helpful Links__
- [Tips on Training with PPO Specifically](https://github.com/yosider/ml-agents-1/blob/master/docs/Training-PPO.md)
- [Tips on Training with SAC Specifically](https://github.com/yosider/ml-agents-1/blob/master/docs/Training-SAC.md)
- [Overview on ML-Agents with some example scenarios](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/ML-Agents-Overview.md)
- [Another implementation of Foosball using ML-Agents.](https://github.com/mbaske/ml-table-football)
  - Be cautious if trying to adapt things from this, as it is on different versions of packages, and was also not designed with the intent of being used to actuate motors
- [Example of adversarial self play in a volleyball environment using ML-Agents](https://github.com/CoderOneHQ/ultimate-volleyball/)
