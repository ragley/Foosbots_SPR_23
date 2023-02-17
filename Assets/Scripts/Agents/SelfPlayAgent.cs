// Author:          Damon Meadows
// Class:           Interdisciplinary Design - Foosbots
// Last Modified:   11-25-2022
// Description:     script to create neural network for ai foosball table

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

    public Agent opAgent;

    private int counter;
    private int endStep;
    private int idleTimer;
    private int maxIdleTime;
    
    private float episodeSumReward;


    float spinPenaltyMult = 0.1f;
    float shotRewardMultiplier = 0.05f;
    bool useSpinPenalty;
    bool useShotReward;

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
    
    Rigidbody enemyAttackRod;

    Rigidbody enemyDefenceRod;
    Rigidbody enemyGoalkeeperRod;
    Rigidbody enemyMidfieldRod;

    

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

        // Initialize utility variables
        initialKick = Vector3.zero;
        autoKick = Vector3.zero;
        useSpinPenalty = false;
        useShotReward = false;
        counter = 0;
        maxIdleTime = 35;
        endStep = 2500;
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
        ball.Reset(Random.Range(-0.000486f, 0.000486f), Random.Range(-0.002689f, 0.002689f));
        initialKick.z = Random.Range(-125f, 125f);
        initialKick.x = Random.Range(-125f, 125f);
        ball.rBody.AddForce(initialKick);

        // reset utility variables
        counter = 0;
        autoKick = Vector3.zero;
        idleTimer = 0;
        episodeSumReward = 0;
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
        sensor.AddObservation(allyAttack0.transform.localRotation.z);
        sensor.AddObservation(allyAttack1.transform.position.z);
        sensor.AddObservation(allyAttack1.transform.position.x);
        sensor.AddObservation(allyAttack1.transform.localRotation.z);
        sensor.AddObservation(allyAttack2.transform.position.z);
        sensor.AddObservation(allyAttack2.transform.position.x);
        sensor.AddObservation(allyAttack2.transform.localRotation.z);

        // Defence Rod
        sensor.AddObservation(allyDefence.transform.position.x);
        sensor.AddObservation(allyDefence.transform.position.z);
        sensor.AddObservation(allyDefence.transform.localRotation.z);
        // Defence Rod Players
        sensor.AddObservation(allyDefence0.transform.position.z);
        sensor.AddObservation(allyDefence0.transform.position.x);
        sensor.AddObservation(allyDefence0.transform.localRotation.z);
        sensor.AddObservation(allyDefence1.transform.position.z);
        sensor.AddObservation(allyDefence1.transform.position.x);
        sensor.AddObservation(allyDefence1.transform.localRotation.z);

        // Goalkeeper Rod
        sensor.AddObservation(allyGoalkeeper.transform.position.x);
        sensor.AddObservation(allyGoalkeeper.transform.position.z);
        sensor.AddObservation(allyGoalkeeper.transform.localRotation.z);
        // GoalKeeper Rod Players 
        sensor.AddObservation(allyGoalkeeper0.transform.position.x);
        sensor.AddObservation(allyGoalkeeper0.transform.position.z);
        sensor.AddObservation(allyGoalkeeper0.transform.localRotation.z);
        sensor.AddObservation(allyGoalkeeper1.transform.position.x);
        sensor.AddObservation(allyGoalkeeper1.transform.position.z);
        sensor.AddObservation(allyGoalkeeper1.transform.localRotation.z);
        sensor.AddObservation(allyGoalkeeper2.transform.position.x);
        sensor.AddObservation(allyGoalkeeper2.transform.position.z);
        sensor.AddObservation(allyGoalkeeper2.transform.localRotation.z);

        // Midfield Rod
        sensor.AddObservation(allyMidfield.transform.position.x);
        sensor.AddObservation(allyMidfield.transform.position.z);
        sensor.AddObservation(allyMidfield.transform.localRotation.z);
        // Midfield Rod Players
        sensor.AddObservation(allyMidfield0.transform.position.x);
        sensor.AddObservation(allyMidfield0.transform.position.z);
        sensor.AddObservation(allyMidfield0.transform.localRotation.z);
        sensor.AddObservation(allyMidfield1.transform.position.x);
        sensor.AddObservation(allyMidfield1.transform.position.z);
        sensor.AddObservation(allyMidfield1.transform.localRotation.z);
        sensor.AddObservation(allyMidfield2.transform.position.x);
        sensor.AddObservation(allyMidfield2.transform.position.z);
        sensor.AddObservation(allyMidfield2.transform.localRotation.z);
        sensor.AddObservation(allyMidfield3.transform.position.x);
        sensor.AddObservation(allyMidfield3.transform.position.z);
        sensor.AddObservation(allyMidfield3.transform.localRotation.z);
        sensor.AddObservation(allyMidfield4.transform.position.x);
        sensor.AddObservation(allyMidfield4.transform.position.z);
        sensor.AddObservation(allyMidfield4.transform.localRotation.z);
 

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
        // action control:
        // set control forces and torques to zero
        Vector3 controlAttackForce = Vector3.zero;
        Vector3 controlAttackTorque = Vector3.zero;
        Vector3 controlDefenceForce = Vector3.zero;
        Vector3 controlDefenceTorque = Vector3.zero;
        Vector3 controlGoalkeeperForce = Vector3.zero;
        Vector3 controlGoalkeeperTorque = Vector3.zero;
        Vector3 controlMidfieldForce = Vector3.zero;
        Vector3 controlMidfieldTorque = Vector3.zero;

        // obtain control forces and torques from network
        controlAttackForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        controlAttackTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        controlDefenceForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        controlDefenceTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);
        controlGoalkeeperForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[4], -1f, 1f);
        controlGoalkeeperTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[5], -1f, 1f);
        controlMidfieldForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[6], -1f, 1f);
        controlMidfieldTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[7], -1f, 1f);

        // apply control forces and torques
        allyAttackRod.AddForce(controlAttackForce);
        allyAttackRod.AddTorque(controlAttackTorque);
        allyDefenceRod.AddForce(controlDefenceForce);
        allyDefenceRod.AddTorque(controlDefenceTorque);
        allyGoalkeeperRod.AddForce(controlGoalkeeperForce);
        allyGoalkeeperRod.AddTorque(controlGoalkeeperTorque);
        allyMidfieldRod.AddForce(controlMidfieldForce);
        allyMidfieldRod.AddTorque(controlMidfieldTorque);

        // rewards:
        //      for self play, one side should always receive negative reward while other receives positive or both get 0
        // reward scoring

        print(allyColor + " Ball in Goal:" + ball.inGoalColor);
        if (ball.inGoalColor == enemyColor)
        {
            episodeSumReward += 5f;
            AddReward(5f);
            print(allyColor + " scored, Episode Reward: " + episodeSumReward);
            opAgent.EndEpisode();
            EndEpisode();
        }
        // penalize being scored on
        if (ball.inGoalColor == allyColor)
        {
            episodeSumReward = -5f;
            print(allyColor + " was scored on, Episode Reward: " + episodeSumReward);
            SetReward(-5f);
            opAgent.EndEpisode();
            EndEpisode();
        }
        

        if (allyColor == PlayerColor.blue)
        {

        }

        if (allyColor == PlayerColor.red)
        {
            
        }

        //punish sitting there
        //AddReward(-0.0001f);
        AddReward(-1f * (1 / endStep));

        if (ballBody.velocity == Vector3.zero)
        {
            idleTimer++;
            if (idleTimer >= maxIdleTime)
            {
                ball.lastKickedColor = PlayerColor.none;
                ballBody.AddForce(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f));
                //SetReward(0f);
                idleTimer = 0;
                //EndEpisode();
            }
        } 
        else
        {
            idleTimer = 0;
        }



        float spin = 0f;
        spin += Mathf.Abs(allyAttackRod.angularVelocity.z);
        spin += Mathf.Abs(allyDefenceRod.angularVelocity.z);
        spin += Mathf.Abs(allyMidfieldRod.angularVelocity.z);
        spin += Mathf.Abs(allyGoalkeeperRod.angularVelocity.z);

        if (useSpinPenalty)
        {
            float spinPenalty = SpinPenalty(spin);
            //print(spinPenalty);
            AddReward(spinPenalty);
            stepSumReward += spinPenalty;
        }
        
        if (ball.lastKickedColor == allyColor)
        {
            if (ball.kicked == true)
            {
                AddReward(0.05f);
                print("KICKED by " + allyColor);
                ball.kicked = false;
            }
            else 
            {
                ball.kicked = false;
            }
        }

        if (useShotReward)
        {
            if (ball.lastKickedColor == allyColor)
            {
                float shotReward = ShotReward();
                print(allyColor + " - Shot Reward: " + shotReward);
                stepSumReward += shotReward;
                AddReward(shotReward);
            }
            
        }



        // end episode after set period
        counter++;
        episodeSumReward += stepSumReward;
        //print("Step Reward: " + stepSumReward);
        //print("=========" + allyColor + " STEP END=========");
        if (counter >= endStep)
        {
            /*            autoKick.x = Random.Range(-125f, 125f);
                        autoKick.z = Random.Range(-125f, 125f);
                        ball.rBody.AddForce(autoKick);*/
            counter = 0;
            episodeSumReward = 0;
            SetReward(episodeSumReward);
            print(allyColor + ": Episode Reward: " + episodeSumReward);
            EndEpisode();
        }

    }


    //TODO: Last Kicked player color Collision with rod rigidbodies

    float SpinPenalty(float spin)
    {
        float penalty = -1f * spinPenaltyMult * spin;
        //print(allyColor + " - Spin Penalty:" + penalty);
        return penalty;
    }


    float ShotReward()
    {
        Vector3 deltaEnemyGoal = enemyGoal.transform.position - ball.transform.position;
        Vector3 deltaAllyGoal = allyGoal.transform.position - ball.transform.position;
        float shotValue = Vector3.Dot(deltaEnemyGoal.normalized, ball.rBody.velocity);
        if (shotValue < 0)
        {
            shotValue = -1f * Vector3.Dot(deltaAllyGoal.normalized, ball.rBody.velocity);
        }

        float reward = shotRewardMultiplier * shotValue;

       // print(allyColor + "- Shot Reward: " + reward);
        return reward;
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
