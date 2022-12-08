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
    private int counter;
    private int endStep;
    Vector3 initialKick;
    Vector3 autoKick;
    Rigidbody ballBody;

    // Ally variables:
    public PlayerColor allyColor;
    public GameObject allyAttack;
    public GameObject allyDefence;
    public GameObject allyGoalkeeper;
    public GameObject allyMidfield;
    public GameObject allyGoal;
    Rigidbody allyAttackRod;
    Rigidbody allyDefenceRod;
    Rigidbody allyGoalkeeperRod;
    Rigidbody allyMidfieldRod;

    // Enemy variables:
    public GameObject enemyGoal;

    // Start up procedures:  
    void Start()
    {
        // Obtain rigidbodies
        allyAttackRod = allyAttack.GetComponent<Rigidbody>();
        allyDefenceRod = allyDefence.GetComponent<Rigidbody>();
        allyGoalkeeperRod = allyGoalkeeper.GetComponent<Rigidbody>();
        allyMidfieldRod = allyMidfield.GetComponent<Rigidbody>();
        ballBody = ball.GetComponent<Rigidbody>();

        // Initialize utility variables
        initialKick = Vector3.zero;
        autoKick = Vector3.zero;
        counter = 0;

        endStep = 1000;
    }

    // Episode initialization:
    public override void OnEpisodeBegin()
    {
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
    }

    // Obtain observations for neural network:
    //      20 observations
    public override void CollectObservations(VectorSensor sensor)
    {
        // ball observations
        sensor.AddObservation(ball.transform.position.x);
        sensor.AddObservation(ball.transform.position.z);
        sensor.AddObservation(ball.rBody.velocity.x);
        sensor.AddObservation(ball.rBody.velocity.z);

        // rod observations
        sensor.AddObservation(allyAttack.transform.position.x);
        sensor.AddObservation(allyAttack.transform.position.z);
        sensor.AddObservation(allyAttack.transform.localRotation.z);

        sensor.AddObservation(allyDefence.transform.position.x);
        sensor.AddObservation(allyDefence.transform.position.z);
        sensor.AddObservation(allyDefence.transform.localRotation.z);

        sensor.AddObservation(allyGoalkeeper.transform.position.x);
        sensor.AddObservation(allyGoalkeeper.transform.position.z);
        sensor.AddObservation(allyGoalkeeper.transform.localRotation.z);

        sensor.AddObservation(allyMidfield.transform.position.x);
        sensor.AddObservation(allyMidfield.transform.position.z);
        sensor.AddObservation(allyMidfield.transform.localRotation.z);

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
        if (ball.inGoalColor != allyColor && ball.inGoalColor != PlayerColor.none)
        {
            AddReward(1f);
            EndEpisode();
        }
        // penalize being scored on
        else if (ball.inGoalColor == allyColor)
        {
            SetReward(-1f);
            EndEpisode();
        }
        // score based on time in forward zone
        if (allyColor == PlayerColor.blue)
        {
            if (ball.transform.localPosition.x > allyAttack.transform.localPosition.x)
            {
                AddReward(.0001f);
            }
        }
        if (allyColor == PlayerColor.red)
        {
            if (ball.transform.localPosition.x < allyAttack.transform.localPosition.x)
            {
                AddReward(.0001f);
            }
        }


        // end episode after set period
        counter++;
        if (counter >= endStep)
        {
            /*            autoKick.x = Random.Range(-125f, 125f);
                        autoKick.z = Random.Range(-125f, 125f);
                        ball.rBody.AddForce(autoKick);*/
            counter = 0;
            EndEpisode();
        }


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
