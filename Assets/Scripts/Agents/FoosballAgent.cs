// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;
// using Unity.MLAgents.Actuators;


// public class FoosballAgent : Agent
// {
//     public Ball ball;
//     public int maxIdleTime;
//     private int idleCounter;
//     private int endCounter;
//     private int endStep;
//     private float ballPosReward;
//     private float goalEnd;
//     private float defEnd;

//     public PlayerColor allyColor;
//     public GameObject allyAttack;
//     public GameObject allyDefence;
//     public GameObject allyGoalkeeper;
//     public GameObject allyMidfield;
//     public GameObject allyGoal;

//     public GameObject enemyAttack;
//     public GameObject enemyDefence;
//     public GameObject enemyGoalkeeper;
//     public GameObject enemyMidfield;
//     public GameObject enemyGoal;

//     Rigidbody allyAttackRod;
//     Rigidbody allyDefenceRod;
//     Rigidbody allyGoalkeeperRod;
//     Rigidbody allyMidfieldRod;

//     Rigidbody enemyAttackRod;
//     Rigidbody enemyDefenceRod;
//     Rigidbody enemyGoalkeeperRod;
//     Rigidbody enemyMidfieldRod;


//     //Vector3 prevBallPosition = new Vector3(0f, 0f, 0f);
//     //Vector3 ballPosition = new Vector3(0f, 0f, 0f);

//     void Start()
//     {
//         allyAttackRod = allyAttack.GetComponent<Rigidbody>();
//         allyDefenceRod = allyDefence.GetComponent<Rigidbody>();
//         allyGoalkeeperRod = allyGoalkeeper.GetComponent<Rigidbody>();
//         allyMidfieldRod = allyMidfield.GetComponent<Rigidbody>();

//         enemyAttackRod = enemyAttack.GetComponent<Rigidbody>();
//         enemyDefenceRod = enemyDefence.GetComponent<Rigidbody>();
//         enemyGoalkeeperRod = enemyGoalkeeper.GetComponent<Rigidbody>();
//         enemyMidfieldRod = enemyMidfield.GetComponent<Rigidbody>();

//         Rigidbody[] allyTeam = { allyAttackRod, allyDefenceRod, allyGoalkeeperRod, allyMidfieldRod };
//         Rigidbody[] enemyTeam = { enemyAttackRod, enemyDefenceRod, enemyGoalkeeperRod, allyMidfieldRod };
        
//         idleCounter = 0;
//         endCounter = 0;
//         endStep = 1000;
//         goalEnd = 28;
//         maxIdleTime = 2;

//     }

//     public override void OnEpisodeBegin()
//     {
//         Debug.Log("Start Episode Foosball Agent");


//         allyAttackRod.velocity = new Vector3(0f, 0f, 0f);
//         allyAttackRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         allyDefenceRod.velocity = new Vector3(0f, 0f, 0f);
//         allyDefenceRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         allyGoalkeeperRod.velocity = new Vector3(0f, 0f, 0f);
//         allyGoalkeeperRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         allyMidfieldRod.velocity = new Vector3(0f, 0f, 0f);
//         allyMidfieldRod.angularVelocity = new Vector3(0f, 0f, 0f);

//         allyAttack.transform.localPosition = new Vector3(-0.002214001f, 0.003497f, 0f);
//         allyDefence.transform.localPosition = new Vector3(0.003690999f, 0.003497f, 0f);
//         allyGoalkeeper.transform.localPosition = new Vector3(0.005166999f, 0.003497f, 0f);
//         allyMidfield.transform.localPosition = new Vector3(0.0007380001f, 0.003497f, 0f);

//         enemyAttackRod.velocity = new Vector3(0f, 0f, 0f);
//         enemyAttackRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         enemyDefenceRod.velocity = new Vector3(0f, 0f, 0f);
//         enemyDefenceRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         enemyGoalkeeperRod.velocity = new Vector3(0f, 0f, 0f);
//         enemyGoalkeeperRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         enemyMidfieldRod.velocity = new Vector3(0f, 0f, 0f);
//         enemyMidfieldRod.angularVelocity = new Vector3(0f, 0f, 0f);
//         /*
//                 ball.Reset(0, Random.Range(-30f, 30f));*/
//         //prevBallPosition = ball.transform.position;
//         //ballPosition = ball.transform.position;

//         idleCounter = 0;
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         sensor.AddObservation(ball.transform.position.x);
//         sensor.AddObservation(ball.transform.position.z);
//         sensor.AddObservation(ball.rBody.velocity.x);
//         sensor.AddObservation(ball.rBody.velocity.z);

//         sensor.AddObservation(allyAttack.transform.position.x);
//         sensor.AddObservation(allyAttack.transform.position.z);
//         sensor.AddObservation(allyAttack.transform.localRotation.z);

//         sensor.AddObservation(allyDefence.transform.position.x);
//         sensor.AddObservation(allyDefence.transform.position.z);
//         sensor.AddObservation(allyDefence.transform.localRotation.z);

//         sensor.AddObservation(allyGoalkeeper.transform.position.x);
//         sensor.AddObservation(allyGoalkeeper.transform.position.z);
//         sensor.AddObservation(allyGoalkeeper.transform.localRotation.z);

//         sensor.AddObservation(allyMidfield.transform.position.x);
//         sensor.AddObservation(allyMidfield.transform.position.z);
//         sensor.AddObservation(allyMidfield.transform.localRotation.z);

//         sensor.AddObservation(allyGoal.transform.position.x);
//         sensor.AddObservation(allyGoal.transform.position.z);

//         sensor.AddObservation(enemyAttack.transform.position.x);
//         sensor.AddObservation(enemyAttack.transform.position.z);

//         sensor.AddObservation(enemyDefence.transform.position.x);
//         sensor.AddObservation(enemyDefence.transform.position.z);

//         sensor.AddObservation(enemyGoalkeeper.transform.position.x);
//         sensor.AddObservation(enemyGoalkeeper.transform.position.z);

//         sensor.AddObservation(enemyMidfield.transform.position.x);
//         sensor.AddObservation(enemyMidfield.transform.position.y);

//         sensor.AddObservation(enemyGoal.transform.position.x);
//         sensor.AddObservation(enemyGoal.transform.position.z);
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         Vector3 controlAttackForce = Vector3.zero;
//         Vector3 controlAttackTorque = Vector3.zero;
//         Vector3 controlDefenceForce = Vector3.zero;
//         Vector3 controlDefenceTorque = Vector3.zero;
//         Vector3 controlGoalkeeperForce = Vector3.zero;
//         Vector3 controlGoalkeeperTorque = Vector3.zero;
//         Vector3 controlMidfieldForce = Vector3.zero;
//         Vector3 controlMidfieldTorque = Vector3.zero;

//         controlAttackForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[0], -3f, 3f);
//         controlAttackTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -3f, 3f);
//         controlDefenceForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[2], -3f, 3f);
//         controlDefenceTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[3], -3f, 3f);
//         controlGoalkeeperForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[4], -3f, 3f);
//         controlGoalkeeperTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[5], -3f, 3f);
//         controlMidfieldForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[6], -3f, 3f);
//         controlMidfieldTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[7], -3f, 3f);

//         allyAttackRod.AddForce(controlAttackForce);
//         allyAttackRod.AddTorque(controlAttackTorque);
//         allyDefenceRod.AddForce(controlDefenceForce);
//         allyDefenceRod.AddTorque(controlDefenceTorque);
//         allyGoalkeeperRod.AddForce(controlGoalkeeperForce);
//         allyGoalkeeperRod.AddTorque(controlGoalkeeperTorque);
//         allyMidfieldRod.AddForce(controlMidfieldForce);
//         allyMidfieldRod.AddTorque(controlMidfieldTorque);

// /*        ballPosition = ball.transform.position;

//         if (ballPosition == prevBallPosition)
//         {
            
//             if (idleCounter == maxIdleTime)
//             {
//                 ball.AutoKick(Random.Range(-10f, 10f), Random.Range(-10, 10f));
//                 idleCounter = 0;
//             }
//             idleCounter += 1;
//         }

//         prevBallPosition = ballPosition;
//         */
        
//         // reward scoring against enemy
//         if (ball.inGoalColor != allyColor && ball.inGoalColor != PlayerColor.none)
//         {
//             AddReward(2f);
//             EndEpisode();
//         }

//         // penalize being scored on
//         else if (ball.inGoalColor == allyColor)
//         {
//             SetReward(-2f);
//             EndEpisode();
//         }

//         // negative reward for doing nothing, drive AI to end faster
//         // AddReward(-0.0001f);

//         // score based on position of ball (RED AGENT)
//         if (ball.transform.position.x > 0.00285f)
//         {
//             AddReward(-.5f);
//         }
//         else if (ball.transform.position.x > 0)
//         {
//             AddReward(-.25f);
//         }
//         else if (ball.transform.position.x > -0.00285f)
//         {
//             AddReward(.5f);
//         }
//         else
//         {
//             AddReward(.25f);
//         }
     
//         //ballPosReward = ((ball.transform.position.x - (-28.5f)) / (28.5f - (-28.5f))) * (0.5f - (-0.5f)) + (-0.5f);
//         //AddReward(ballPosReward);

//         /*        endCounter += 1;
//                 if (endCounter == endStep)
//                 {
//                     EndEpisode();
//                 }*/

//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         var continuousActionsOut = actionsOut.ContinuousActions;
//         continuousActionsOut[0] = Input.GetAxis("Vertical");
//         continuousActionsOut[1] = Input.GetAxis("Horizontal");
//         continuousActionsOut[2] = Input.GetAxis("Vertical");
//         continuousActionsOut[3] = Input.GetAxis("Horizontal");
//         continuousActionsOut[4] = Input.GetAxis("Vertical");
//         continuousActionsOut[5] = Input.GetAxis("Horizontal");
//         continuousActionsOut[6] = Input.GetAxis("Vertical");
//         continuousActionsOut[7] = Input.GetAxis("Horizontal");
//     }

// }
