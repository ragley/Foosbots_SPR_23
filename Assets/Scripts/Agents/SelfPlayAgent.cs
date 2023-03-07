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


public class SelfPlayAgent : Agent
{
    // Utility variables:
    public Ball ball;

    public SelfPlayAgent opAgent;
    public bool goalOccur =  false;

    private int counter;
    private int endStep;
    private int idleTimer;
    
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
    public float timeStepPenalty; // usually (-1 * 1/maxstep)
    public float timeStep;

    public int maxIdleTime;
    public int decisionInterval;
    public float idleSpeedThreshold;

    public bool useSpinPenalty;
    public bool useSingleShotReward;
    public bool useContShotReward;
    public bool useKickReward;
    public bool regKicks;
    public bool regGoals;
    public bool inputPosition;
    public bool isPlaying;


    
    
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

  

    // Enemy variables:

    public PlayerColor enemyColor;

    public GameObject enemyAttack;
    public GameObject enemyAttack0;
    public GameObject enemyAttack1;
    public GameObject enemyAttack2;
    
    public GameObject enemyDefence;
    public GameObject enemyDefence0;
    public GameObject enemyDefence1;

    public GameObject enemyGoalkeeper;
    public GameObject enemyGoalkeeper0;
    public GameObject enemyGoalkeeper1;
    public GameObject enemyGoalkeeper2;


    public GameObject enemyMidfield;
    public GameObject enemyMidfield0;
    public GameObject enemyMidfield1;
    public GameObject enemyMidfield2;
    public GameObject enemyMidfield3;
    public GameObject enemyMidfield4;
    
    public GameObject enemyGoal;
    float endReward = 0f;
    
    Rigidbody enemyAttackRod;
    Rigidbody enemyDefenceRod;
    Rigidbody enemyGoalkeeperRod;
    Rigidbody enemyMidfieldRod;

    

    // Start up procedures:  
    void Start()
    {
        Application.targetFrameRate = 60;
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

        // Initialize utility variables

        goalOccur = false;
        initialKick = Vector3.zero;
        autoKick = Vector3.zero;
        counter = 0;
        maxIdleTime = 60;
        endStep = 500;
    }

    // Episode initialization:
    public override void OnEpisodeBegin()
    {
        //Debug.Log("Start New Episode - Self-Play Agent: " + allyColor);

        // domain randomization
        ballBody.mass = Random.Range(.995f, 1f);
        ballBody.drag = Random.Range(0f, .005f);
        ballBody.angularDrag = Random.Range(.048f, .052f);
        allyAttackRod.mass = Random.Range(.995f, 1f);
        allyAttackRod.drag = Random.Range(0f, .005f);
        allyAttackRod.angularDrag = Random.Range(.048f, .052f);
        allyDefenceRod.mass = Random.Range(.995f, 1f);
        allyDefenceRod.drag = Random.Range(0f, .005f);
        allyDefenceRod.angularDrag = Random.Range(.048f, .052f);
        allyGoalkeeperRod.mass = Random.Range(.995f, 1f);
        allyGoalkeeperRod.drag = Random.Range(0f, .005f);
        allyGoalkeeperRod.angularDrag = Random.Range(.048f, .052f);
        allyMidfieldRod.mass = Random.Range(.995f, 1f);
        allyMidfieldRod.drag = Random.Range(0f, .005f);
        allyMidfieldRod.angularDrag = Random.Range(.048f, .052f);

        // reset rod velocities
        allyAttackRod.velocity = new Vector3(0f, 0f, 0f);
        allyAttackRod.angularVelocity = new Vector3(0f, 0f, 0f);
        allyDefenceRod.velocity = new Vector3(0f, 0f, 0f);
        allyDefenceRod.angularVelocity = new Vector3(0f, 0f, 0f);
        allyGoalkeeperRod.velocity = new Vector3(0f, 0f, 0f);
        allyGoalkeeperRod.angularVelocity = new Vector3(0f, 0f, 0f);
        allyMidfieldRod.velocity = new Vector3(0f, 0f, 0f);
        allyMidfieldRod.angularVelocity = new Vector3(0f, 0f, 0f);

        // reset rod positions based on allyColor
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

        // reset ball to random position between midfield rods and apply small autokick
        goalOccur = false;
        ball.Reset(Random.Range(-0.000486f, 0.000486f), Random.Range(-0.002689f, 0.002689f));
        initialKick.z = Random.Range(-125f, 125f);
        initialKick.x = Random.Range(-125f, 125f);
        ball.rBody.AddForce(initialKick);

        // reset utility variables

        counter = 0;
        autoKick = Vector3.zero;
        idleTimer = 0;
        episodeSumRewards = 0f;
        episodeShotRewards = 0f;
        episodeSpinPenalties = 0f;
        episodeTimePenalties = 0f;
        episodeKickRewards = 0f;

        
    }

    // Obtain observations for neural network:
    //      89 observations
    public override void CollectObservations(VectorSensor sensor)
    {
        // ball observations
        sensor.AddObservation(ball.transform.position.x);
        sensor.AddObservation(ball.transform.position.z);
        sensor.AddObservation(ball.rBody.velocity.x);
        sensor.AddObservation(ball.rBody.velocity.z);

        // Ally Rod Observations
        // Attack Rod
        sensor.AddObservation(allyAttack.transform.position.x);
        sensor.AddObservation(allyAttack.transform.position.z);
        sensor.AddObservation(allyAttack.transform.localRotation.z);
        // Attack Rod Players (L -> R from ALLY goal perspective)
        sensor.AddObservation(allyAttack0.transform.position.z);
        sensor.AddObservation(allyAttack0.transform.position.x);
        sensor.AddObservation(allyAttack1.transform.position.z);
        sensor.AddObservation(allyAttack1.transform.position.x);
        sensor.AddObservation(allyAttack2.transform.position.z);
        sensor.AddObservation(allyAttack2.transform.position.x);

        // Midfield Rod
        sensor.AddObservation(allyMidfield.transform.position.x);
        sensor.AddObservation(allyMidfield.transform.position.z);
        sensor.AddObservation(allyMidfield.transform.localRotation.z);
        // Midfield Rod Players
        sensor.AddObservation(allyMidfield0.transform.position.x);
        sensor.AddObservation(allyMidfield0.transform.position.z);
        sensor.AddObservation(allyMidfield1.transform.position.x);
        sensor.AddObservation(allyMidfield1.transform.position.z);
        sensor.AddObservation(allyMidfield2.transform.position.x);
        sensor.AddObservation(allyMidfield2.transform.position.z);
        sensor.AddObservation(allyMidfield3.transform.position.x);
        sensor.AddObservation(allyMidfield3.transform.position.z);
        sensor.AddObservation(allyMidfield4.transform.position.x);
        sensor.AddObservation(allyMidfield4.transform.position.z);
 
        // Defence Rod
        sensor.AddObservation(allyDefence.transform.position.x);
        sensor.AddObservation(allyDefence.transform.position.z);
        sensor.AddObservation(allyDefence.transform.localRotation.z);
        // Defence Rod Players
        sensor.AddObservation(allyDefence0.transform.position.z);
        sensor.AddObservation(allyDefence0.transform.position.x);
        sensor.AddObservation(allyDefence1.transform.position.z);
        sensor.AddObservation(allyDefence1.transform.position.x);

        // Goalkeeper Rod
        sensor.AddObservation(allyGoalkeeper.transform.position.x);
        sensor.AddObservation(allyGoalkeeper.transform.position.z);
        sensor.AddObservation(allyGoalkeeper.transform.localRotation.z);
        // GoalKeeper Rod Players 
        sensor.AddObservation(allyGoalkeeper0.transform.position.x);
        sensor.AddObservation(allyGoalkeeper0.transform.position.z);
        sensor.AddObservation(allyGoalkeeper1.transform.position.x);
        sensor.AddObservation(allyGoalkeeper1.transform.position.z);
        sensor.AddObservation(allyGoalkeeper2.transform.position.x);
        sensor.AddObservation(allyGoalkeeper2.transform.position.z);
        
        
        // Enemy Rod Observations
        // Attack Rod
        sensor.AddObservation(enemyAttack.transform.position.x);
        // Attack Rod Players (L -> R from ALLY goal perspective)
        sensor.AddObservation(enemyAttack0.transform.position.z);
        sensor.AddObservation(enemyAttack0.transform.position.x);
        sensor.AddObservation(enemyAttack1.transform.position.z);
        sensor.AddObservation(enemyAttack1.transform.position.x);
        sensor.AddObservation(enemyAttack2.transform.position.z);
        sensor.AddObservation(enemyAttack2.transform.position.x);

        // Midfield Rod
        sensor.AddObservation(enemyMidfield.transform.position.x);

        // Midfield Rod Players
        sensor.AddObservation(enemyMidfield0.transform.position.x);
        sensor.AddObservation(enemyMidfield0.transform.position.z);
        sensor.AddObservation(enemyMidfield1.transform.position.x);
        sensor.AddObservation(enemyMidfield1.transform.position.z);
        sensor.AddObservation(enemyMidfield2.transform.position.x);
        sensor.AddObservation(enemyMidfield2.transform.position.z);
        sensor.AddObservation(enemyMidfield3.transform.position.x);
        sensor.AddObservation(enemyMidfield3.transform.position.z);
        sensor.AddObservation(enemyMidfield4.transform.position.x);
        sensor.AddObservation(enemyMidfield4.transform.position.z);

        // Defence Rod
        sensor.AddObservation(enemyDefence.transform.position.x);
        // Defence Rod Players
        sensor.AddObservation(enemyDefence0.transform.position.z);
        sensor.AddObservation(enemyDefence0.transform.position.x);
        sensor.AddObservation(enemyDefence1.transform.position.z);
        sensor.AddObservation(enemyDefence1.transform.position.x);

        // Goalkeeper Rod
        sensor.AddObservation(enemyGoalkeeper.transform.position.x);
        // GoalKeeper Rod Players 
        sensor.AddObservation(enemyGoalkeeper0.transform.position.x);
        sensor.AddObservation(enemyGoalkeeper0.transform.position.z);
        sensor.AddObservation(enemyGoalkeeper1.transform.position.x);
        sensor.AddObservation(enemyGoalkeeper1.transform.position.z);
        sensor.AddObservation(enemyGoalkeeper2.transform.position.x);
        sensor.AddObservation(enemyGoalkeeper2.transform.position.z);

        
        // goal observations
        sensor.AddObservation(allyGoal.transform.position.x);
        sensor.AddObservation(allyGoal.transform.position.z);

        sensor.AddObservation(enemyGoal.transform.position.x);
        sensor.AddObservation(enemyGoal.transform.position.z);
    
    }

    // Main driver function of neural network:
    //      takes actions
    //      handles rewards
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

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


        // obtain control forces and torques from network
        controlAttackForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        controlAttackTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        controlMidfieldForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        controlMidfieldTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);        
        controlDefenceForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[4], -1f, 1f);
        controlDefenceTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[5], -1f, 1f);
        controlGoalkeeperForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[6], -1f, 1f);
        controlGoalkeeperTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[7], -1f, 1f);


        // Different output methods from network, testing for methods of physical motor function
        if (inputPosition)
        {
            //apply control velocities
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
            //apply control forces and torques
            allyAttackRod.AddForce(controlAttackForce);
            allyAttackRod.AddTorque(controlAttackTorque);
            allyMidfieldRod.AddForce(controlMidfieldForce);
            allyMidfieldRod.AddTorque(controlMidfieldTorque);            
            allyDefenceRod.AddForce(controlDefenceForce);
            allyDefenceRod.AddTorque(controlDefenceTorque);
            allyGoalkeeperRod.AddForce(controlGoalkeeperForce);
            allyGoalkeeperRod.AddTorque(controlGoalkeeperTorque);

        }
        

        

        // rewards:
        //      for self play, one side should always receive negative reward while other receives positive or both get 0
        // reward scoring

        //print(allyColor + " Ball in Goal:" + ball.inGoalColor);
        

        // Set reward to 1 if negative overall because it still scored
        // With self play one side must be positive and the other negative, so cannot have negative reward if it "Wins"
        // Note: Not reslly sure how exactly self play reward signaling works, need more tuning and research, conflicting arguments in docu. 

        // isPlaying function that can easily disable one player for curriculum training instead of self play
        if (isPlaying)
        {
            // Check if agent is registering goals
            // Note: should always have one agent who is registering goals
            if (regGoals == true){    
                
                // Reward scoring by goal value
                if (ball.inGoalColor == enemyColor)
                {                    
                    endReward = goalRewardValue;     
                    endType = "Goal by " + allyColor + "Recorded. Ending current Episode. ";               
                    
                    // If the autokick goes in the enemy goal, don't give the team points for that, but note if it goes in the ally
                    // goal, it still counts as a score because we want it to not let itself be scored on                    
                    if (ball.lastKickedColor == PlayerColor.none){
                        endReward = 0f;
                        endType = "AutoKick Score By: " + allyColor + ", No points given. ";
                    }
                    
                    // print(allyColor + " scored");
                    goalOccur = true;
                }

                // penalize being scored on by negative of goal value
                if (ball.inGoalColor == allyColor)
                {
                    endReward = -1f * goalRewardValue;
                    goalOccur = true;
                    print(allyColor + " was scored on");
                }
                
                // For single rod play (early curriculum training) override the goal occur variable 
                // since the opponent will not update to true on it's own
                if (opAgent.regGoals == false)
                {
                    // Set opponent's goal occur to true
                    singlePLayGoalDetectOverRide();
                }
            }

            // Placeholder Conditional for potential differences in team actions
            if (allyColor == PlayerColor.blue)
            {

            }

            if (allyColor == PlayerColor.red)
            {
                
            }

            // === STEP REWARD ===
            // Spin Penalty
            // Sum of angular velocities of rods
            float spin = 0f;
            spin += Mathf.Abs(allyAttackRod.angularVelocity.z);
            spin += Mathf.Abs(allyDefenceRod.angularVelocity.z);
            spin += Mathf.Abs(allyMidfieldRod.angularVelocity.z);
            spin += Mathf.Abs(allyGoalkeeperRod.angularVelocity.z);

            // Penalty based on the angular velocity sum (higher is worse)
            if (useSpinPenalty == true)
            {
                float spinPenalty = SpinPenalty(spin);
                //print(spinPenalty);
                episodeSpinPenalties += spinPenalty;
                stepSumReward += spinPenalty;
            }
            
            // === STEP REWARD ===
            // Kick and Shot Judgement Rewards
            if (regKicks == true)
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

                        print("KICKED by " + allyColor);
                        
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
            }


            // === STEP REWARD ===
            // Existential penalty
            stepSumReward += -1f * timeStepPenalty; 
            episodeTimePenalties += -1f * timeStepPenalty;
        
        }


        // Add the value of the rewards for the current step to the episode total
        stepSumReward += endReward;
        episodeSumRewards += stepSumReward;
        
        // Make sure both agents recognize the goal happened to avoid race condition, 
        // then end the episode for both agents so they start from the same point
        if(goalOccur == true && opAgent.goalOccur == true)
        {    
            print(SummaryStr(endType));
            goalOccur = false;
            opAgent.goalOccur = false;
            opAgent.EndEpisode();
            EndEpisode();
        }

        // Add the Reward for the current step as the sum of the acquired rewards and penalies
        // Note: This is ADD reward, rewards accumulate between decision periods and only then are added to the model
        // SET reward will overwrite the accumulated reward the next time it is called, so it cannot be used with our decision requester or every
        // reward/penalty that happens in between will be lost
        
        AddReward(stepSumReward);
        //print("Step Reward - " + allyColor + ": " + stepSumReward);
        
        // End episode after set period
        counter++;
        if (counter >= endStep)
        {
            if (opAgent.isPlaying == false)
            {
                // If the opponent isn't playing, in a timeout, just keep the rewards the same for the episode
                endType = "Timeout. ";
                print(SummaryStr(endType));
                EndEpisode();
            }
            else
            {
                // TODO: Fix Draw Condition for Self Play!!!!
                // For self play, draw condition should be zero for both parties
                //episodeSumReward = 0;
                //SetReward(episodeSumReward);
                EndEpisode();
            }
           
            
        }

        // Idle Kick
        // TODO: Possibly make function of speed with threshold instead of fixed zero
        if (ballBody.velocity.magnitude <= idleSpeedThreshold)
        {
            idleTimer++;
            if (idleTimer >= maxIdleTime)
            {
               IdleKick();
            }
        } 
        else
        {
            idleTimer = 0;
        }
    }

    void IdleKick()
    {
        ball.lastKickedColor = PlayerColor.none;
        print("Ball Auto Kick - Player color set to: " + ball.lastKickedColor);
        ballBody.AddForce(Random.Range(-125f, 125f), 0, Random.Range(-125f, 125f));
        //SetReward(0f);
        idleTimer = 0;
        //EndEpisode();
    }

    float SpinPenalty(float spin)
    {
        float penalty = -1f * spinPenaltyMult * spin;
        //print(allyColor + " - Spin Penalty:" + penalty);
        return penalty;
    }


    float ShotReward()
    {

        //TODO: Refine Scoring Vectors, more accurate 
        // forward/towards enemy goal = good
        // backwards/towards own goal = bad

        //Vector from current ball positon to enemy goal: "Perfect Shot"
        Vector3 deltaEnemyGoal = enemyGoal.transform.position - ball.transform.position;
        //Vector from current ball position to ally goal: "Worst Shot"
        Vector3 deltaAllyGoal = allyGoal.transform.position - ball.transform.position;
        
        // Calc Shot value based on vector towards center of enemy goal
        float shotValue = Vector3.Dot(deltaEnemyGoal.normalized, ball.rBody.velocity);
        // If it's negative apply penalty based on vector towards ally goal ("don't hit it towards your own goal")
        if (shotValue < 0)
        {
            //shotValue = 0;
            shotValue = -1f * Mathf.Abs(Vector3.Dot(deltaAllyGoal.normalized, ball.rBody.velocity));
        }
        
        float reward = shotRewardMultiplier * shotValue;

       // print(allyColor + "- Shot Reward: " + reward);
        return reward;
    }


    public void singlePLayGoalDetectOverRide()
    {
        opAgent.goalOccur = true;
    }

    // String for summary of rewards at the end of an episode for debug
    public string SummaryStr(string endType)
    {
        string str = endType + " Summary: Rewards: " + allyColor + " Total Sum: " + episodeSumRewards + " Goal: " + endReward +  " Kick: " + episodeKickRewards + " Shots: " + episodeShotRewards +
        " Spin Penalties: " + episodeSpinPenalties + " Time Penalties: " + episodeTimePenalties;
        
        return str;   
    }

    //TODO: Very Slightly Randomize Velocities?
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
        
        if (Mathf.Abs(vel) > 250f)
        {
            vel = (vel / Mathf.Abs(vel)) * 250f;
        }
        
        velV.z = vel;
        return velV;
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
