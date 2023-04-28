# Foosbots_Fall_22
This project leverages the Unity game engine and ML Agents, an open source package for Unity, to train a neural network via reinforcement learning to play the game of foosball.
<br></br>

## Getting Started
First, you will need to install Unity 2021.3.11 which can be found at the following link: <https://unity.com/releases/editor/archive>

In order to utilize the precise version of python required, it is recommended that all work be done within a virtual environment. An easy guide for setting up a virtual environment can be found here: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Virtual-Environment.md>

Once your virtual environment is activated follow these steps to install all necessary technologies:

  1.) Install Python 3.7.7 which can be found at the following link: <https://www.python.org/downloads/release/python-377/>
  
  2.) Install PyTorch by running the following command from the command line:
  
      pip install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
    
  3.) Install ML Agents' Python package by running the following command from the command line:
  
      python -m pip install mlagents==0.28.0
  4.) Install imutils PythonPackage
      
      pip install "need actual command"
  5.) Download the Unity Assets Hosted in the MS Teams files. They are too large to be hosted here unfortunately, and git LFS does not function for public repositories.
<br></br>
  
## Training the Network
In order to train the network, open the project in Unity and then open a command prompt terminal. From within the terminal, navigate to the directory containing the Unity project and activate your virtual environment. Then run the following command:

    mlagents-learn config/foosball_config.yaml --run-id TITLE_OF_TRAINING_RUN
    
This should prompt you to click play in the Unity editor, do so and the training will commence. Replace TITLE_OF_TRAINING_RUN with whatever you wish to call that particular session of training. 

To edit the hyperparameters of the network, open the file foosball_config.yaml and make any necessary changes, then save the file. More information about the hyperparameters and their various effects on training can be found at the following link: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md>
<br></br>

## Important Components of Agent Script
There are several agent scripts included in the project from the various phases of testing and developing the project. The one that is currently being used and is most up to date is called `SelfPlayAgentJoint.cs`. Public variables should be set in the `TableEnvHandler.cs` to ensure settings are the same for both agents. All agents have the following important methods:
<br></br>

### `OnEpisodeBegin()` - Void
This method is called at the start of every training episode. Currently the only thing here is a counter to display how many times the `OnActionsRecieved()` function is called.
<br></br>

### `CollectObservations()` - Void
This method is utilized to obtain observations for the neural network and is called every `FixedUpdate()`. Every observation must be a single number. So, if you wish to pass something like the (x,z) coordinates of the ball's location, this would be two separate observations. Observations undergo Domain Randomization, with upper and lower multipliers defined as public variables, and are put into an array, which is then jooped over and applied to the `AddSensor()` function. 

**IMPORTANT:** Observations for the agent should ***ALWAYS*** be from the perspective of the **ALLY** goal. Functions have been created to do this, they are `getGoalRelPos()` and `getGoalRelDir()`. This keeps the observations symmetric for both agents. If this convention is not followed, it's likely you'll end up with an agent brain that plays properly on one team but not the same on the other. This is **BAD**, as the physical table AI Teams are on different sides.
<br></br>

### `OnActionReceived()` - Void
This method is the main driver function of the agent in charge of both producing actions and producing rewards, this function is called every interval set in `Decision Requester`, so, `OnActionsRecieved()` is called every `DecisionInterval` `FixedUpdate()` steps. This project makes use of a continuous action space where all outputs are naturally in the range (-1,1) but we manually clip them to guarantee that they stay in that range, per recommendation of the ML Agents team. Currently the actions of the agent are in the form of "Desired Position" or where the agent wants to be in terms of its minimum and maximum actuation values for the specific action. 

The reward structure is designed to work with curriculum based training or Self Play, depending on settings defined in the unity build and training configuration .yaml file. Rewards are designed to be symmetric in their application, meaning you should be able to train either red or blue team individually (Or both in terms of Self Play), without modifying the reward code. 

Rewards in this section can be tricky. The way the simulation is currently set up, ONLY rewards that affect a single agent should be placed in this section. If any rewards here are defined based on the outcome of the other agent's actions, a race condition will develop.

*For Example:*
- Scoring
  - Should NOT be in `OnActionsRecieved()`.
  - Scoring, by definition, should end the episode. If the episode end is called here, it will skip the agent who doesn't register the score first, and thus must be handled in the `TableEnvHandler.cs`
- Shot Rewards
  - Should be in the Agent Script in `OnActionsRecieved()`
  - A the outcome or evaluation of a shot an agent makes has no relation to the other agent, and it's direct outcome will not affect the opponent agent.
<br></br>

### ``ShotReward()`` - Float
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

The resultant float value is multiplied by a user-defined `shotRewardMultiplier` and returned. I've found with `useNegShotPenalty=true`, `usePosessionEval=false`, and `useSingleShotReward=false`, results in a purely symmetric reward system: if red gets `+X` blue gets exactly `-X` for the shot reward at any given time.   
<br></br>

### `SpinPenalty()` - Float
This function is to prevent the rods from endlessly spinning as fast as possible, as previously the AI would find this is the best solution to getting the ball in the goal as fast as possible.

*This function currently needs further testing and balancing and/or is obsolete. Currently Rods are unable to fully spin more than 354 degrees (-177, 177)*
<br></br>

### `SummaryStr(string endType)` - String
This is a function for debugging to print out the total reward recieved by the agent at the end of each episode, as well as individual reward breakdowns. It can be called from a function `TableEnvHandler.cs` to display breakdowns of both agents.
<br></br>

### `desPosition(int rod, float inputDesiredPos, string space)` - Float
This function converts the desired position output by the neural network `(-1, 1)` to a value that corresponds to a linear position for each rod. The `space` parameter can be either `global` or `local` depending on the context this function is used in, and will return values in that space.
<br></br>

### `getRodVelLinear(int rod, Vector3 curPos, float inputDesiredPos)` - Vector3
This is a very important function as it's responsible for actuating the rods in a similar manner to the methods used by the physical table. Since the physical table is actuated by stepper motors, operating on desired position commands, the unity simulation is required to actuate in a similar manner. The difficulty with this is maintaining a continuous physics simulation that allows for consistent collisions. The rods could be easily actuated with the `Transform.position` and `Transform.rotation` commands within Unity but these ignore the physics system. Taking inspiration from the [paper published by KIcker](https://www.researchgate.net/publication/341204434_KIcker_An_Industrial_Drive_and_Control_Foosball_System_automated_with_Deep_Reinforcement_Learning), the decision was made to utilize target velocities.

The function takes in the current `Vector3` Position the selected rod, as well as calls `desPosition()` to get the neural network's desired `z` position. Both these values are converted to relative space to the ally goal. It then calculates the velocity required to get the rod from the current position, to the desired position in one timestep, capped at a specific velocity, ideally set to a value correspondig to the table's max actuation speed.
<br></br>

### `getRodVelRot(float curPos, float inputDesiredPos)` - Vector3
This function works similarly to `getRodVelLiner()` as it takes the desired position from the neural network and normalizes the value to `(-180, 180)`, then calculates the velocity required to reach the desired position in one timestep.
<br></br>

### `getGoalRelPos(Vector3 obj)` - Vector3
This function returns a `Vector3` object corresponding to the given position in relative space of the agent's ally goal.
<br></br>

### `getGoalRelDir(Vector3 obj)` - Vector3
This function returns a `Vector3` object corresponding to the given direction in relative space of the agent's ally goal. Useful for direction vectors like velocity.
<br></br>

### `getHitAngularVelocity(Collision collisionData)` - Float
Returns a float corresponding to the angular velocity of the rod that makes collision with the ball.
<br></br>

### `Reset()` - Void
This function resets the agent's position to the default values, i.e. centered with a rotation of 0 (feet straight down). It is currently called from the `Reset()` function in `TableEnvHandler.cs`.
<br></br>

### `Heuristic()` - Void
This method is used for manual testing. When "heuristic only" is selected within the "behavior parameters" section in the Unity editor, the network will not take actions on its own and instead the user can use the arrow keys to move the rods.
<br></br>

## Important Components of TableEnvHandler Script
`needs content`
<br></br>

## Important Components of Agent in Unity Editor
In addition to the script, every game object that is being used as an agent must have the following components
<br></br>

### Multiple Table Formation
`needs content`
<br></br>

### Actuating Components
`needs content`
<br></br>

### Behvaior Parameters
This component determines how an agent makes decisions and  has several important sub-components `NEEDS PIC UPDATE`

![foosball_behavior_parameters](https://user-images.githubusercontent.com/35296087/206737538-0e228616-4cff-4555-ba08-375b74c02f06.png)

`Behavior Name:` This field must match the behavior named specified in the .yaml file referenced during training

`Vector Observation Space:` The number of elements in the `CollectObservations()` method, we have 46 in our script.

`Actions:` The size of the action space, for this project there are 8 continuous action outputs (2 per rod, translation, rotation) and 0 discrete actions.

`Model`: Allows for the option of loading a brain for inference. Put the brains in `Assets/Brains/` to have them show up. (Make sure to name them someting easily identifiable!)

`Behavior Type:` What type of behavior the AI has. 
- `Default` Will either run inference with the selected brain if not training, or if `mlagents-learn` is active it will train.
- `Inference Only`: Like it says, will always run inference on selected device, useful if training one team only.
- `Heuristic Only`: Like it says, useful for controlling rods yourself and seeing actuation behaviors.


`Team ID:` Which team each agent is on, it is important for the self play training that each team have a different ID.

### Decision Requester
This component determines how often the agent makes decisions within a training run

![foosball_decison_requester](https://user-images.githubusercontent.com/35296087/206738764-6a268c05-15dd-489e-bd67-06885dccc74e.png)

Note that "Take Actions Between" is unchecked and that the `Decision Period > 1` ***NEEDS CHECK***, we found that when allowed to make decisions at every step the agent would get stuck moving its rods only in one direction but when it was slightly throttled it had full range of motion. In the main simulation this can be set to whatever, but it's recommended that when running inference on the table it be set to 1.
<br></br>

## Accessing and Evaluating Results
In order to view tensorboard graphs to evaluate training, activate the virtual environment and navigate to the directory containing the Unity project then run the following command:

    tensorboard --logdir results --port 6006
  
Then, navigate to [localhost:6006](http://localhost:6006).

There are 7 statistics used primarily to evaluate the network

### Cumulative Reward
This measures the mean cumulative reward over all agents and should increase through training
### Policy Loss
This correlates to how much the policy is changing throughout training and should decrease over time
### Value Loss
This correlates to how well the model can predict the value of each state and should initially increase but ultimately stabilize
### Policy Entropy
This correlates to how random the model's decisions are and should decrease over the course of training
### Policy Learning Rate
This correlates to how large a step the algorithm is taking as it searches for an optimal policy and should decrease over time
### Policy Value Estimate
This measures the mean estimated value for all states visited by the agent and should be expected to increase over time
### ELO
This measures the relative skill between two players and in a proper training run this should steadily increase

Notes: 
- "test_documentation.txt" contains the various hyperparameters used on each of the included training runs.
- Sometimes it takes a while to display new values especially with long training runs saved, these add up to several Gb of data. Just keep refreshing or go grab a coffee, it can take 5-10 minutes to display sometimes.

More information about the use of tensorboard and evaluation of these statisitics can be found at the following link: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Tensorboard.md>
<br></br>

## Deployment on the Physical Table

### Input Observations
Fourty-six obervations are assigned in SelfPlay, and then passed along to the Inference file. 
These inputs can be viewed in Unity in the *Dummy Inference (Script)* section within the  **Inspector** tab. 
All these observations are slightly randomized.

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
<br></br>

### Neural Network Decisions 
This array of 8 objects is defined in SelfPlay within the *OnActionRecieved()* function, and passed to Inference. 

| Index  | Object                      | Action    |
| ------ | --------------------------- |---------- |
| 0      | allyAttack (3-Rod)          | Force     |
| 1      | allyAttack (3-Rod)          | Torque    |
|        |                             |           |
| 2      | allyMidfield (5-Rod)        | Force     |
| 3      | allyMidfield (5-Rod)        | Torque    |
|        |                             |           |
| 4      | allyDefence (2-Rod)         | Force     |
| 5      | allyDefence (2-Rod)         | Torque    |
|        |                             |           |
| 6      | allyGoalkeeper (goal-Rod)   | Force     |
| 7      | allyGoalkeeper (goal-Rod)   | Torque    |
<br></br>

### Output Commands
This array is the output decisions of the NN. These are defined in the Inference file. Max actuation, player spacing, and the conversion required for Unity -> IRL are all accounted for in these outputs. 

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
<br></br>


## Helpful Links
- [Tips on Training with PPO Specifically](https://github.com/yosider/ml-agents-1/blob/master/docs/Training-PPO.md)
- [Tips on Training with SAC Specifically](https://github.com/yosider/ml-agents-1/blob/master/docs/Training-SAC.md)
- [Overview on ML-Agents with some example scenarios](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/ML-Agents-Overview.md)
- [Another implementation of Foosball using ML-Agents.](https://github.com/mbaske/ml-table-football)
  - Be cautious if trying to adapt things from this, as it is on different versions of packages, and was also not designed with the intent of being used to actuate motors
- [Example of adversarial self play in a volleyball environment using ML-Agents](https://github.com/CoderOneHQ/ultimate-volleyball/)
