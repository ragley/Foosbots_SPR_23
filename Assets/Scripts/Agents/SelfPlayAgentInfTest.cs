// Author:          Damon Meadows
// Class:           Interdisciplinary Design - Foosbots
// Last Modified:   11-25-2022
// Description:     script to create neural network for ai foosball table

// Add: It seems that ml-agents depends on the old importlib-metadata library. I solved this issue by  pip install importlib-metadata==4.4 to readme
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SelfPlayAgentInfTest : Agent
{


int MIN_PLAYER_OFFSET = 40;
int MAX_PLAYER_OFFSET = 640;


// Unity
float U_TABLE_LENGTH_MAX = 76f;
float U_TABLE_LENGTH_MIN = 0f;
float U_TABLE_WIDTH_MAX = 1.5f;
float U_TABLE_WIDTH_MIN = -1.5f;

// Dimensions of the Table balltracking (these are VERY important) (mm)
float TABLE_LENGTH = 1193.8f;
float TABLE_WIDTH = 694.2f;

// PHYSICAL DIMENSIONS
Dictionary<string, int> GOAL_ROD = new Dictionary<string, int>(){{"maxActuation", 228} , {"playerSpacing", 182}, {"rodX",1125}, {"numPlayers",3}};
Dictionary<string, int> TWO_ROD = new Dictionary<string, int>(){{"maxActuation",356}, {"playerSpacing",237}, {"rodX",975}, {"numPlayers",2}};
Dictionary<string, int> FIVE_ROD = new Dictionary<string, int>(){{"maxActuation",115}, {"playerSpacing",120}, {"rodX",675}, {"numPlayers",5}};
Dictionary<string, int> THREE_ROD = new Dictionary<string, int>(){{"maxActuation",181}, {"playerSpacing",207}, {"rodX",375}, {"numPlayers",3}};
Dictionary<string, int> TABLE = new Dictionary<string, int>(){{"robot_goalX",1200},{"robot_goalY",350}, {"player_goalX",0}, {"player_goalY",350}, {"goalWidth",200}, {"width",685}, {"length",1200}};



    // Utility variables:
    public Ball ball;

    public SelfPlayAgentInfTest opAgent;
    public bool goalOccur =  false;

    private int counter;
    private int endStep;
    private int idleTimer;
    public int MaxEnvSteps;
    
    private float episodeSpinPenalties;
    private float episodeShotRewards;
    private float episodeTimePenalties;
    private float episodeSumRewards;
    private float episodeKickRewards;
    private string endType;


    public float spinPenaltyMult;
    public float shotRewardMultiplier;
    public float kickReward;
    public float goalRewardValue;
    public float timeStepPenalty;
    public float timeStep;

    //public int maxIdleTime;
    public int decisionInterval;
    //public float idleSpeedThreshold;

    public bool useSpinPenalty;
    public bool useTimePenalty;
    public bool useSingleShotReward;
    public bool useContShotReward;
    public bool useKickReward;
    public bool usePossessionEval;
    public bool regKicks;
    public bool regGoals;
    public bool inputPosition;
    public bool isPlaying;
    public bool useNegShotPenalty;

    //public bool episodeEndSignal;


    
    
    Vector3 initialKick;
    Vector3 autoKick;
    Rigidbody ballBody;

    // Ally variables:
    public PlayerColor allyColor;
    
    public GameObject allyAttack;
    public GameObject allyAttack0;
    public GameObject allyAttack1;
    public GameObject allyAttack2;


    public GameObject allyDefence;
    public GameObject allyDefence0;
    public GameObject allyDefence1;

    public GameObject allyGoalkeeper;
    public GameObject allyGoalkeeper0;
    public GameObject allyGoalkeeper1;
    public GameObject allyGoalkeeper2;

    public GameObject allyMidfield;
    public GameObject allyMidfield0;
    public GameObject allyMidfield1;
    public GameObject allyMidfield2;
    public GameObject allyMidfield3;
    public GameObject allyMidfield4;
    
    public GameObject allyGoal;
    
    Rigidbody allyAttackRod;
    Rigidbody allyDefenceRod;
    Rigidbody allyGoalkeeperRod;
    Rigidbody allyMidfieldRod;
    public int onactcalls;

  

    // Enemy variables:

    public PlayerColor enemyColor;

    public GameObject enemyAttack;
    // public GameObject enemyAttack0;
    // public GameObject enemyAttack1;
    // public GameObject enemyAttack2;
    
    public GameObject enemyDefence;
    // public GameObject enemyDefence0;
    // public GameObject enemyDefence1;

    public GameObject enemyGoalkeeper;
    // public GameObject enemyGoalkeeper0;
    // public GameObject enemyGoalkeeper1;
    // public GameObject enemyGoalkeeper2;


    public GameObject enemyMidfield;
    // public GameObject enemyMidfield0;
    // public GameObject enemyMidfield1;
    // public GameObject enemyMidfield2;
    // public GameObject enemyMidfield3;
    // public GameObject enemyMidfield4;
    
    public GameObject enemyGoal;
    float endReward;
    
    Rigidbody enemyAttackRod;
    Rigidbody enemyDefenceRod;
    Rigidbody enemyGoalkeeperRod;
    Rigidbody enemyMidfieldRod;

    public Vector3 posTest;
    public float ballVZ;
    public float ballVX;
    public Vector3 ballV;
    public Vector3 ballPos;
    public float enemyGoalPosX;
    public int maxAngularVelocity;
    
    public float cumReward;
    public UIManager ui;                                                 

    // Start up procedures:  
    void Start()
    {

        // Obtain rigidbodies
        allyAttackRod = allyAttack.GetComponent<Rigidbody>();
        allyDefenceRod = allyDefence.GetComponent<Rigidbody>();
        allyGoalkeeperRod = allyGoalkeeper.GetComponent<Rigidbody>();
        allyMidfieldRod = allyMidfield.GetComponent<Rigidbody>();
        
        enemyAttackRod = enemyAttack.GetComponent<Rigidbody>();
        enemyDefenceRod = enemyDefence.GetComponent<Rigidbody>();
        enemyGoalkeeperRod = enemyGoalkeeper.GetComponent<Rigidbody>();
        enemyMidfieldRod = enemyMidfield.GetComponent<Rigidbody>();
        
        ballBody = ball.GetComponent<Rigidbody>();

        allyAttackRod.maxAngularVelocity = maxAngularVelocity;
        allyDefenceRod.maxAngularVelocity = maxAngularVelocity;
        allyMidfieldRod.maxAngularVelocity = maxAngularVelocity;
        allyGoalkeeperRod.maxAngularVelocity = maxAngularVelocity;



        ui = GameObject.Find("UIManager").GetComponent<UIManager>(); 

    }

    // Episode initialization:
    public override void OnEpisodeBegin()
    {
        onactcalls = 0;
    }

    // Obtain observations for neural network:
    // 46 - old table operation
    

    public override void CollectObservations(VectorSensor sensor)
    {
        // Only make observations if agent is playing, not sure if this works
        if (isPlaying == true)
        {
            // Observations are relative to ally team's goal, this way they can be symmetric regardless of side

            sensor.AddObservation(getGoalRelPos(ball.transform.position).x);
            sensor.AddObservation(getGoalRelPos(ball.transform.position).z);
            sensor.AddObservation(ball.ballRB.velocity.x);
            sensor.AddObservation(ball.ballRB.velocity.z);

            // Ally Rod Observations
            // Attack Rod
            sensor.AddObservation(getGoalRelPos(allyAttack.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyAttack.transform.position).z);
            sensor.AddObservation(allyAttack.transform.localRotation.z);
            // Attack Rod Players (L -> R from ALLY goal perspective)
            sensor.AddObservation(getGoalRelPos(allyAttack0.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyAttack0.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyAttack1.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyAttack1.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyAttack2.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyAttack2.transform.position).z);

            // Midfield Rod
            sensor.AddObservation(getGoalRelPos(allyMidfield.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield.transform.position).z);
            sensor.AddObservation(allyMidfield.transform.localRotation.z);
            // Midfield Rod Players
            sensor.AddObservation(getGoalRelPos(allyMidfield0.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield0.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyMidfield1.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield1.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyMidfield2.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield2.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyMidfield3.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield3.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyMidfield4.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyMidfield4.transform.position).z);

            // Defence Rod
            sensor.AddObservation(getGoalRelPos(allyDefence.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyDefence.transform.position).z);
            sensor.AddObservation(allyDefence.transform.localRotation.z);
            // Defence Rod Players
            sensor.AddObservation(getGoalRelPos(allyDefence0.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyDefence0.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyDefence1.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyDefence1.transform.position).z);

            // Goalkeeper Rod
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper.transform.position).z);
            sensor.AddObservation(allyGoalkeeper.transform.localRotation.z);
            // GoalKeeper Rod Players 
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper0.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper0.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper1.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper1.transform.position).z);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper2.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyGoalkeeper2.transform.position).z);
            
            
            // // Enemy Rod Observations, unfeasible at the moment
            // // Attack Rod
            // sensor.AddObservation(getGoalRelPos(enemyAttack.transform.position).x);
            // // Attack Rod Players (L -> R from ALLY goal perspective)
            // sensor.AddObservation(getGoalRelPos(enemyAttack0.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyAttack0.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyAttack1.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyAttack1.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyAttack2.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyAttack2.transform.position).x);

            // // Midfield Rod
            // sensor.AddObservation(getGoalRelPos(enemyMidfield.transform.position).x);

            // // Midfield Rod Players
            // sensor.AddObservation(getGoalRelPos(enemyMidfield0.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield0.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield1.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield1.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield2.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield2.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield3.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield3.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield4.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyMidfield4.transform.position).z);

            // // Defence Rod
            // sensor.AddObservation(getGoalRelPos(enemyDefence.transform.position).x);
            // // Defence Rod Players
            // sensor.AddObservation(getGoalRelPos(enemyDefence0.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyDefence0.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyDefence1.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyDefence1.transform.position).x);

            // // Goalkeeper Rod
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper.transform.position).x);
            // // GoalKeeper Rod Players 
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper0.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper0.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper1.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper1.transform.position).z);
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper2.transform.position).x);
            // sensor.AddObservation(getGoalRelPos(enemyGoalkeeper2.transform.position).z);

            
            // goal observations
            sensor.AddObservation(getGoalRelPos(allyGoal.transform.position).x);
            sensor.AddObservation(getGoalRelPos(allyGoal.transform.position).z);

            sensor.AddObservation(getGoalRelPos(enemyGoal.transform.position).x);
            sensor.AddObservation(getGoalRelPos(enemyGoal.transform.position).z);
        }
    }

    // Main driver function of neural network:
    //      takes actions
    //      handles rewards
    // called 1 time per decision step (set in decision requester)
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        onactcalls += 1;
        haltVelocity();
        //allyAttackRod.constraints = RigidbodyConstraints.FreezeRotationZ;
        //allyAttackRod.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;


        // Debug for ball observations
        // ballPos = getGoalRelPos(ball.transform.position);
        // ballVZ = getGoalRelDir(ball.ballRB.velocity).z;
        // ballVX = getGoalRelDir(ball.ballRB.velocity).x;
        // ballV = getGoalRelDir(ball.ballRB.velocity);


        //print("=========" + allyColor + " STEP START=========");
        float stepSumReward = 0f;
        endReward = 0f;
        
        // action control:
        // set control forces and torques to zero
        Vector3 controlAttackForce = Vector3.zero;
        Vector3 controlAttackTorque = Vector3.zero;
        Vector3 controlMidfieldForce = Vector3.zero;
        Vector3 controlMidfieldTorque = Vector3.zero;
        Vector3 controlDefenceForce = Vector3.zero;
        Vector3 controlDefenceTorque = Vector3.zero;
        Vector3 controlGoalkeeperForce = Vector3.zero;
        Vector3 controlGoalkeeperTorque = Vector3.zero;

        // isPlaying function that can easily disable one player for curriculum training instead of self play
        if (isPlaying)
        {
            // obtain control forces and torques from network
            controlAttackForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
            controlAttackTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
            controlMidfieldForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
            controlMidfieldTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);        
            controlDefenceForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[4], -1f, 1f);
            controlDefenceTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[5], -1f, 1f);
            controlGoalkeeperForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[6], -1f, 1f);
            controlGoalkeeperTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[7], -1f, 1f);

            // Flip Red's outputs because they should be mirrored to what blue would do in the same scenario
            if (allyColor == PlayerColor.red)
            {
                controlAttackForce.z *= -1;
                controlAttackTorque.z *= -1;
                controlMidfieldForce.z *= -1;
                controlMidfieldTorque.z *= -1;        
                controlDefenceForce.z *= -1;
                controlDefenceTorque.z *= -1;
                controlGoalkeeperForce.z *= -1;
                controlGoalkeeperTorque.z *= -1;
            }
            
            // Different output methods from network, testing for methods of physical motor function
            if (inputPosition)
            {
                //apply control velocities (Direct Method)
                allyAttackRod.velocity = getRodVelLinear(0, allyAttackRod.transform.position.z, controlAttackForce.z);
                allyAttackRod.angularVelocity = getRodVelRot(allyAttackRod.transform.localRotation.z, controlAttackTorque.z);
                allyMidfieldRod.velocity = getRodVelLinear(1, allyMidfieldRod.transform.position.z, controlMidfieldForce.z);
                allyMidfieldRod.angularVelocity = getRodVelRot(allyMidfieldRod.transform.localRotation.z, controlMidfieldTorque.z);            
                allyDefenceRod.velocity = getRodVelLinear(2, allyDefenceRod.transform.position.z, controlDefenceForce.z);
                allyDefenceRod.angularVelocity = getRodVelRot(allyDefenceRod.transform.localRotation.z, controlDefenceTorque.z);
                allyGoalkeeperRod.velocity = getRodVelLinear(0, allyGoalkeeperRod.transform.position.z, controlGoalkeeperForce.z);
                allyGoalkeeperRod.angularVelocity = getRodVelRot(allyGoalkeeperRod.transform.localRotation.z, controlGoalkeeperTorque.z);

            } else
            {
                //apply control forces and torques (Unity Method)
                allyAttackRod.AddForce(getRodVelLinear(0, allyAttackRod.transform.position.z, controlAttackForce.z), ForceMode.VelocityChange);
                allyAttackRod.AddTorque(getRodVelRot(allyAttackRod.transform.localRotation.z, controlAttackTorque.z), ForceMode.VelocityChange);
                allyMidfieldRod.AddForce(getRodVelLinear(1, allyMidfieldRod.transform.position.z, controlMidfieldForce.z), ForceMode.VelocityChange);
                allyMidfieldRod.AddTorque(getRodVelRot(allyMidfieldRod.transform.localRotation.z, controlMidfieldTorque.z), ForceMode.VelocityChange);            
                allyDefenceRod.AddForce(getRodVelLinear(2, allyDefenceRod.transform.position.z, controlDefenceForce.z), ForceMode.VelocityChange);
                allyDefenceRod.AddTorque(getRodVelRot(allyDefenceRod.transform.localRotation.z, controlDefenceTorque.z), ForceMode.VelocityChange);
                allyGoalkeeperRod.AddForce(getRodVelLinear(0, allyGoalkeeperRod.transform.position.z, controlGoalkeeperForce.z), ForceMode.VelocityChange);
                allyGoalkeeperRod.AddTorque(getRodVelRot(allyGoalkeeperRod.transform.localRotation.z, controlGoalkeeperTorque.z), ForceMode.VelocityChange);

            }
            
            
            // rewards:
            //      for self play, one side should always receive negative reward while other receives positive or both get 0
            
            // reward scoring ==> Handled by TableEnvHandler.cs

            //print(allyColor + " Ball in Goal:" + ball.inGoalColor);
            // Set reward to 1 if negative overall because it still scored
            // With self play one side must be positive and the other negative, so cannot have negative reward if it "Wins"
            // Note: Not reslly sure how exactly self play reward signaling works, need more tuning and research, conflicting arguments in docu. 
            
            // Check if agent is registering goals
            // Note: should always have one agent who is registering goals
            if (regGoals == true){    
                
                // Reward scoring by goal value
                if (ball.inGoalColor == enemyColor)
                {                    
                    endReward = goalRewardValue;     
                    // endType = "Goal by " + allyColor + " Recorded. Ending current Episode. ";
                    // elapsedTime = Time.realtimeSinceStartup;  

                    // if(ball.inGoalColor == PlayerColor.red)     { ui.scoreBlue();}  // Add to Blue's score on the UI panel
                    // if(ball.inGoalColor == PlayerColor.blue)    { ui.scoreRed();}   // Add to Red's score on the UI panel        
                    
                    // // If the autokick goes in the enemy goal, don't give the team points for that, but note if it goes in the ally
                    // // goal, it still counts as a score because we want it to not let itself be scored on                    
                    // if (ball.lastKickedColor == PlayerColor.none){
                    //     endReward = 0f;
                    //     endType = "AutoKick Score By: " + allyColor + ", No points given. ";
                    // }
                    
                    // // print(allyColor + " scored");
                    // goalOccur = true;
                }

                // penalize being scored on by negative of goal value
                if (ball.inGoalColor == allyColor)
                {
                    endReward = -1f * goalRewardValue;
                    // goalOccur = true;
                    // endType = allyColor + " was scored on. Ending current Episode. ";               
                }
                
                // For single rod play (early curriculum training) override the goal occur variable 
                // since the opponent will not update to true on it's own
                // if (opAgent.regGoals == false)
                // {
                //     // Set opponent's goal occur to true
                //     singlePLayGoalDetectOverRide();
                // }
            }

            // // Placeholder Conditional for potential differences in team actions
            // if (allyColor == PlayerColor.blue)
            // {

            // }

            // if (allyColor == PlayerColor.red)
            // {
                
            // }
        


            // Spin Penalty
            // Sum of angular velocities of rods
            // float spin = 0f;



            // spin += Mathf.Abs(allyAttackRod.angularVelocity.z);
            // spin += Mathf.Abs(allyDefenceRod.angularVelocity.z);
            // spin += Mathf.Abs(allyMidfieldRod.angularVelocity.z);
            // spin += Mathf.Abs(allyGoalkeeperRod.angularVelocity.z);

            // Penalty based on the angular velocity sum (higher is worse)
            if (useSpinPenalty == true)
            {
                float spinPenalty = SpinPenalty();
                //print(spinPenalty);
                episodeSpinPenalties += spinPenalty;
                stepSumReward += spinPenalty;
            }
            
            // === STEP REWARD ===
            // Kick and Shot Judgement Rewards
            if (regKicks == true)
            {
                if (usePossessionEval == true)
                {
                    if (ball.lastKickedColor == allyColor)
                    {
                        if (ball.kicked == true)
                        {
                            // Optional Reward for simply kicking the ball with a player
                            if (useKickReward)
                            {
                                stepSumReward += kickReward;
                                episodeKickRewards += kickReward;                        
                            }

                            //print("KICKED by " + allyColor);
                            
                            // Optional Reward for shot based on direction of ball after being kicked by an ally (See ShotReward Function)
                            // Note: Might be helpful for the ai initially learning to hit the ball
                            if (useSingleShotReward)
                            {
                                float singleShotReward = ShotReward();
                                print(allyColor + " - Shot Reward: " + singleShotReward);
                                stepSumReward += singleShotReward;
                                episodeShotRewards += singleShotReward;
                            } 
                            
                            ball.kicked = false;
                        }
                        else 
                        {
                            ball.kicked = false;
                        }

                        // Optional Reward for continuous reward signaling every step based on shots
                        // Ex: Ai hits it off the wall directly in the goal
                        //  Single shot won't consider this a good kick but when the ball changes direction to travel towards the goal
                        //  this function will reward that shot
                        // Note: This funcin will also work the same as the single shot the moment the ball is hit
                        if (useContShotReward)
                        {
                            float contShotReward = ShotReward();
                            //print(allyColor + " - Shot Reward: " + contShotReward);
                            stepSumReward += contShotReward;
                            episodeShotRewards += contShotReward;
                        } 
                    }
                } else 
                {
                    float contShotReward = ShotReward();
                    //print(allyColor + " - Shot Reward: " + contShotReward);
                    stepSumReward += contShotReward;
                    episodeShotRewards += contShotReward;
                }
            }


            // === STEP REWARD ===
            // Existential penalty
            if (useTimePenalty == true)
            {
                stepSumReward += timeStepPenalty; 
                episodeTimePenalties += timeStepPenalty;
            }

        
        }


        // Add the value of the rewards for the current step to the episode total
        //stepSumReward += endReward;
        episodeSumRewards += stepSumReward;
        
        // Add the Reward for the current step as the sum of the acquired rewards and penalies
        // Note: This is ADD reward, rewards accumulate between decision periods and only then are added to the model
        // SET reward will overwrite the accumulated reward the next time it is called, so it cannot be used with our decision requester or every
        // reward/penalty that happens in between will be lost
        
        AddReward(stepSumReward);

        // Make sure both agents recognize the goal happened to avoid race condition, 
        // then end the episode for both agents so they start from the same point
        // if(goalOccur == true && opAgent.goalOccur == true)
        // {    
        //     print(SummaryStr(endType));
        //     goalOccur = false;
        //     opAgent.goalOccur = false;
        //     episodeEndSignal = true;
        // }



        //print("Step Reward - " + allyColor + ": " + stepSumReward);
        
        // End episode after set period ==> Handled in TableEnvHandler.cs
        // counter++;
        // if (counter >= endStep)
        // {
        //     if (opAgent.isPlaying == false)
        //     {
        //         // If the opponent isn't playing, in a timeout, just keep the rewards the same for the episode
        //         endType = "Timeout. ";
        //         print(SummaryStr(endType));
        //         EndEpisode();
        //     }
        //     else
        //     {
        //         // TODO: Fix Draw Condition for Self Play!!!!
        //         // For self play, draw condition should be zero for both parties
        //         //episodeSumReward = 0;
        //         //SetReward(episodeSumReward);
        //         EndEpisode();
        //     }
           
            
        // }

        cumReward = GetCumulativeReward();

        
    }
        // Idle Kick
        // TODO: Possibly make function of speed with threshold instead of fixed zero
    //     if (ballBody.velocity.magnitude <= idleSpeedThreshold)
    //     {
    //         idleTimer++;
    //         if (idleTimer >= maxIdleTime)
    //         {
    //            IdleKick();
    //         }
    //     } 
    //     else
    //     {
    //         idleTimer = 0;
    //     }
    // }

    // void IdleKick()
    // {
    //     ball.lastKickedColor = PlayerColor.none;
    //     print("Ball Auto Kick - Player color set to: " + ball.lastKickedColor);
    //     ballBody.AddForce(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f), ForceMode.VelocityChange);
    //     //SetReward(0f);
    //     idleTimer = 0;
    //     //EndEpisode();
    // }

    float SpinPenalty()
    {
                    // Sum of angular velocities of rods
            float spin = 0f;
            
            Vector3 locAttackAngularVel = allyAttack.transform.InverseTransformVector(allyAttackRod.angularVelocity);
            Vector3 locDefenceAngularVel = allyDefence.transform.InverseTransformVector(allyDefenceRod.angularVelocity);
            Vector3 locMidfieldAngularVel = allyMidfield.transform.InverseTransformVector(allyMidfieldRod.angularVelocity);
            Vector3 locGoalkeeperAngularVel = allyGoalkeeper.transform.InverseTransformVector(allyGoalkeeperRod.angularVelocity);


            spin += Mathf.Abs(locAttackAngularVel.z);
            spin += Mathf.Abs(locDefenceAngularVel.z);
            spin += Mathf.Abs(locMidfieldAngularVel.z);
            spin += Mathf.Abs(locGoalkeeperAngularVel.z);

        float penalty = -1f * spinPenaltyMult * spin;
        //print(allyColor + " - Spin Penalty:" + penalty);
        return penalty;
    }


    float ShotReward()
    {

        // TODO: Refine Scoring Vectors, more accurate 
        // forward/towards enemy goal = good
        // backwards/towards own goal = bad

        float shotValue = 0;

        // Vector from current ball positon to enemy goal: "Perfect Shot"
        Vector3 deltaEnemyGoal = getGoalRelPos(enemyGoal.transform.position) - getGoalRelPos(ball.transform.position);
        // Vector from current ball position to ally goal: "Worst Shot"
        Vector3 deltaAllyGoal = getGoalRelPos(allyGoal.transform.position) - getGoalRelPos(ball.transform.position);
        
        // Calc Shot value based on vector towards center of enemy goal
        float shotOnEnemyValue = Vector3.Dot(deltaEnemyGoal.normalized, getGoalRelDir(ball.ballRB.velocity));
        float shotOnAllyValue = Vector3.Dot(deltaAllyGoal.normalized, getGoalRelDir(ball.ballRB.velocity));
        // If it's negative apply penalty based on vector towards ally goal ("don't hit it towards your own goal")
        if (useNegShotPenalty == true)
        {    
            if (shotOnAllyValue > shotOnEnemyValue)
            {
                shotValue = -1f * shotOnAllyValue;

            } else if (shotOnEnemyValue >= shotOnAllyValue)
            {
                shotValue = shotOnEnemyValue;
            }
        } else 
        {
            if (shotOnEnemyValue >= 0)
            {
                shotValue = shotOnEnemyValue;
            } else
            {
                shotValue = 0;
            }
        }

        float reward = shotRewardMultiplier * shotValue;

       // print(allyColor + "- Shot Reward: " + reward);
        return reward;
    }


    // public void singlePLayGoalDetectOverRide()
    // {
    //     opAgent.goalOccur = true;
    // }

    // String for summary of rewards at the end of an episode for debug
    public string SummaryStr(string endType)
    {
        string str = endType + " Summary: Rewards: " + allyColor + " Total Sum: " + GetCumulativeReward() + " Goal: " + endReward +  " Kick: " + episodeKickRewards + " Shots: " + episodeShotRewards +
        " Spin Penalties: " + episodeSpinPenalties + " Time Penalties: " + episodeTimePenalties;
        
        return str;   
    }

    //TODO: Very Slightly Randomize Velocities?
    //TODO: CHANGE TO RELATIVE TO GOAL
    public Vector3 getRodVelLinear(int rod, float curPos, float inputDesiredPos)
    {
       
        // Cur is in global
        // Velocity is global so cur and desired need to be converted to global
        float vel = 0f;
        float rounded = Mathf.Round(curPos * 1000.0f) / 1000.0f;
        curPos = rounded;
        //print("id: " + rod);
        //print("cur:" + curPos);
        Vector3 velV = Vector3.zero;
        float desPos = 0f;
        // Make debug script to show both local and global coordinates, find bounds of movement, replace the ones int the function
        if (rod == 0)
        {
            // Attack Rod or Goal (Both 3 Man Rods)
            // Pos between +- 0.001175 LOCAL +- 11.65 global
            desPos =  11.64f * inputDesiredPos;

        } else if (rod == 1)
        {
            // Midfield Rod
            // Pos between +- 0.000625 +-6.1
            desPos =  5.8f * inputDesiredPos;
            
            
        } else if (rod == 2)
        {
            // Defence Rod
            // Pos between +- 0.00165 +- 16.33
            desPos =  16.32f * inputDesiredPos;
            
        } else 
        {
            // Invalid Rod ID case
            velV.z = 0f;
        }
        //print("des: " + desPos);
        vel = (desPos - curPos) / (timeStep);
        if (Mathf.Abs(vel) > 24f)
        {
            vel = (vel / Mathf.Abs(vel)) * 24f;
        }
        //print("vel: " + vel);
        velV.z = vel;
        return velV;
    }

    public Vector3 getRodVelRot(float curPos, float inputDesiredPos)
    {
        // TODO: possibly limit the amount of travel per turn?
        float vel = 0f;
        Vector3 velV = Vector3.zero;
        
        // pos input in degrees, velocity in rad/s
        // AI output is bound between -1,1 so normalize to between -180,180
        float desPos = 180 * inputDesiredPos; 

        // Conversion factor of degrees to rad
        float toRad = (3.14159265f / 180f);
        
        // position change
        float dX = desPos - curPos;

        // check the shortest way to desired position and go that direction
        // if ((dX + 360) % 360 < 180)
        // {
        //     vel = -(dX * toRad) / (1 * timeStep);
        // }
        // else 
        // {
        //     vel = -1f * (dX * toRad) / (1 * timeStep); //Possible Decision interval isnt needed, not sure yet
        // }
        
        vel = (dX * toRad) / (1 * timeStep);
        
        if (Mathf.Abs(vel) > 125f)
        {
            vel = (vel / Mathf.Abs(vel)) * 125f;
        }
        
        velV.z = vel;
        return velV;
    }

    // public int getTableLinPosition(string Rod, float nn_Lin){
    //     // NN Output -> Phys Table Values -> Unity Value Output
    //     // NN Output [-1,1]
        
    //     int maxActuation = Rod["maxActuation"];
    //     int irl_lin = ((nn_Lin - (-1)) / (1 - 1 (-1))) * maxActuation;

         

    // }

    public Vector3 getGoalRelPos(Vector3 obj)
    {
        // Get position of the object relative to the ally goal
        Vector3 pos = allyGoal.transform.InverseTransformPoint(obj);
        return pos;
    }

    public Vector3 getGoalRelDir(Vector3 obj)
    {
        // Get position of the object relative to the ally goal
        Vector3 dir = allyGoal.transform.InverseTransformDirection(obj);
        return dir;
    }

    public void Reset()
    {
        if (allyColor == PlayerColor.red)
        {
            allyAttack.transform.localPosition = new Vector3(-0.002214001f, 0.003497f, 0f);
            allyDefence.transform.localPosition = new Vector3(0.003690999f, 0.003497f, 0f);
            allyGoalkeeper.transform.localPosition = new Vector3(0.005166999f, 0.003497f, 0f);
            allyMidfield.transform.localPosition = new Vector3(0.0007380001f, 0.003497f, 0f);
        }

        if (allyColor == PlayerColor.blue)
        {
            allyAttack.transform.localPosition = new Vector3(0.002214001f, 0.003497f, 0f);
            allyDefence.transform.localPosition = new Vector3(-0.003690999f, 0.003497f, 0f);
            allyGoalkeeper.transform.localPosition = new Vector3(-0.005166999f, 0.003497f, 0f);
            allyMidfield.transform.localPosition = new Vector3(-0.0007380001f, 0.003497f, 0f);
        }

        // reset rod rotations
        allyAttack.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        allyDefence.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        allyGoalkeeper.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        allyMidfield.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

        // reset utility variables
        
        goalOccur = false;
        //episodeEndSignal = false;
        counter = 0;
        autoKick = Vector3.zero;
        idleTimer = 0;
        episodeSumRewards = 0f;
        episodeShotRewards = 0f;
        episodeSpinPenalties = 0f;
        episodeTimePenalties = 0f;
        episodeKickRewards = 0f;
    
    }

    public void haltVelocity()
    {
        // allyAttackRod.drag = 1e9f;
        // allyAttackRod.angularDrag = 1e9f;
        // allyMidfieldRod.drag = 1e9f;
        // allyMidfieldRod.angularDrag = 1e9f;
        // allyDefenceRod.drag = 1e9f;
        // allyDefenceRod.angularDrag = 1e9f;
        // allyGoalkeeperRod.drag = 1e9f;
        // allyGoalkeeperRod.angularDrag = 1e9f;

        // allyAttackRod.AddForce((-1f * allyAttackRod.velocity), ForceMode.VelocityChange);
        // allyAttackRod.AddTorque((-1f * allyAttackRod.angularVelocity), ForceMode.VelocityChange);
        // allyMidfieldRod.AddForce((-1f * allyMidfieldRod.velocity), ForceMode.VelocityChange);
        // allyMidfieldRod.AddTorque((-1f * allyMidfieldRod.angularVelocity), ForceMode.VelocityChange);
        // allyDefenceRod.AddForce((-1f * allyDefenceRod.velocity), ForceMode.VelocityChange);
        // allyDefenceRod.AddTorque((-1f * allyDefenceRod.angularVelocity), ForceMode.VelocityChange);
        // allyGoalkeeperRod.AddForce((-1f * allyGoalkeeperRod.velocity), ForceMode.VelocityChange);
        // allyGoalkeeperRod.AddTorque((-1f * allyGoalkeeperRod.angularVelocity), ForceMode.VelocityChange);

        // allyAttackRod.drag = 0;
        // allyAttackRod.angularDrag = 0;
        // allyMidfieldRod.drag = 0;
        // allyMidfieldRod.angularDrag = 0;
        // allyDefenceRod.drag = 0;
        // allyDefenceRod.angularDrag = 0;
        // allyGoalkeeperRod.drag = 0;
        // allyGoalkeeperRod.angularDrag = 0;
    }
    
    // Manual driver function for testing:
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        continuousActionsOut[2] = Input.GetAxis("Vertical");
        continuousActionsOut[3] = Input.GetAxis("Horizontal");
        continuousActionsOut[4] = Input.GetAxis("Vertical");
        continuousActionsOut[5] = Input.GetAxis("Horizontal");
        continuousActionsOut[6] = Input.GetAxis("Vertical");
        continuousActionsOut[7] = Input.GetAxis("Horizontal");
    }
}
