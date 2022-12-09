# Foosbots_Fall_22
This project leverages the Unity game engine and ML Agents, an open source package for Unity, to train a neural network via reinforcement learning to play the game of foosball.

## Getting Started
First, you will need to install Unity 2021.3.11 which can be found at the following link: <https://unity.com/releases/editor/archive>

In order to utilize the precise version of python required, it is recommended that all work be done within a virtual environment. An easy guide for setting up a virtual environment can be found here: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Virtual-Environment.md>

Once your virtual environment is activated follow these steps to install all necessary technologies:

  1.) Install Python 3.7.7 which can be found at the following link: <https://www.python.org/downloads/release/python-377/>
  
  2.) Install PyTorch by running the following command from the command line:
  
      pip install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
    
  3.) Install ML Agents' Python package by running the following command from the command line:
  
      python -m pip install mlagents==0.30.0
      
  
## Training the Network
In order to train the network, open the project in Unity and then open a command prompt terminal. From within the terminal, navigate to the directory containing the Unity project and activate your virtual environment. Then run the following command:

    mlagents-learn config/foosball_config.yaml --run-id TITLE_OF_TRAINING_RUN
    
This should prompt you to click play in the Unity editor, do so and the training will commence. Replace TITLE_OF_TRAINING_RUN with whatever you wish to call that particular session of training. 

To edit the hyperparameters of the network, open the file foosball_config.yaml and make any necessary changes, then save the file. More information about the hyperparameters and their various effects on training can be found at the following link: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md>

## Important Components of Agent Script
There are several agent scripts included in the project from the various phases of testing and developing the project. The one that is currently being used and is most up to date is called "SelfPlayAgent". All agents have the following important methods
### OnEpisodeBegin()
This method is called at the start of every training episode and is used both to reset the various game objects within the Unity scene as well as to perform the technique of domain randomization and introduce noise into the training run in order to help bypass the reality gap.
### CollectObservations()
This method is utilized to obtain observations for the neural network. Every observation must be a single number. So, if you wish to pass something like the (x,z) coordinates of the ball's location, this would be two separate observations.
### OnActionReceived()
This method is the main driver function of the agent in charge of both producing actions and producing rewards. This project makes use of a continuous action space where all outputs are naturally in the range (-1,1) but we manually clip them to guarantee that they stay in that range, per recommendation of the ML Agents team. The reward structure is designed to work with the self play training routine which requires that one team be awarded positive points and the other negative or both teams receive zero points. We found that purely rewarding scoring and penalizing being scored upon was not sufficient for growth of the network's ELO rating (the main indication of improvement in a self play setting) so we have small added rewards for getting the ball close to the enemy goal. However, since the losing team is required to receive a negative evaluation, their reward is always set to -1 and thus only the winning team receives reward signals based on ball proximity to goal.
### Heuristic()
This method is used for manual testing. When "heuristic only" is selected within the "behavior parameters" section in the Unity editor, the network will not take actions on its own and instead the user can use the arrow keys to move the rods.

## Important Components of Agent in Unity Editor
In addition to the script, every game object that is being used as an agent must have the following components
### Behvaior Parameters
This component determines how an agent makes decisions and  has several important sub-components

![foosball_behavior_parameters](https://user-images.githubusercontent.com/35296087/206737538-0e228616-4cff-4555-ba08-375b74c02f06.png)

Vector Observation Space: The number of elements in the CollectObservations() method, we have 20 in our script

Actions: The size of the action space, for this project there are 8 continuous action outputs (2 per rod) and 0 discrete actions

Team ID: Which team each agent is on, it is important for the self play training that each team have a different ID
### Decision Requester
This component determines how often the agent makes decisions within a training run

![foosball_decison_requester](https://user-images.githubusercontent.com/35296087/206738764-6a268c05-15dd-489e-bd67-06885dccc74e.png)

Note that "Take Actions Between" is unchecked and that the "Decision Period" is >1, we found that when allowed to make decisions at every step the agent would get stuck moving its rods only in one direction but when it was slightly throttled it had full range of motion.
## Accessing and Evaluating Results
In order to view tensorboard graphs to evaluate training, activate the virtual environment and navigate to the directory containing the Unity project then run the following command:

    tensorboard --logdir results --port 6006
  
Then, navigate to [localhost:6006](http://localhost:6006).

There are 7 statistics used primarily to evaluate the network
### ELO
This measures the relative skill between two players and in a proper training run this should steadily increase
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

Note: "test_documentation.txt" contains the various hyperparameters used on each of the included training runs

More information about the use of tensorboard and evaluation of these statisitics can be found at the following link: <https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Tensorboard.md>
