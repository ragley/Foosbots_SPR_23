using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DummyAgent : Agent
{
    //Define Observation Variables
    // Make Public so can be watched in unity editor and accessed

    public float allyAttackLinearCommand;
    public float allyAttackRotationCommand;
    public float allyMidfieldLinearCommand;
    public float allyMidfieldRotationCommand;
    public float allyDefenceLinearCommand;
    public float allyDefenceRotationCommand;
    public float allyGoalkeeperLinearCommand;
    public float allyGoalkeeperRotationCommand; 
    public int[] currentScore = new int[2];
    public int[] prevScore = new int [2];
    public float[] actions = new float[8];
    public float[] inputs = new float[46];
    public DummyInference DI;
    //debug
    public int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentScore[0] = DI.robot_score;
        currentScore[1] = DI.player_score;

    }

    public override void OnEpisodeBegin()
    {
        currentScore[0] = DI.robot_score;
        currentScore[1] = DI.player_score;
        //count++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        inputs = DI.inputs;
        //count++;
        for (int i = 0; i < inputs.Length; i++) {
                sensor.AddObservation(inputs[i]);
            }

        // sensor.AddObservation(ball_x);
        // sensor.AddObservation(ball_z);
        // sensor.AddObservation(ball_Vx);
        // sensor.AddObservation(ball_Vz);

        // // Ally Rod Observations
        // // Attack Rod
        // sensor.AddObservation(allyAttack_x);
        // sensor.AddObservation(allyAttack_z);
        // sensor.AddObservation(allyAttack_Rot_z);
        // // Attack Rod Players (L -> R from ALLY goal perspective)
        // sensor.AddObservation(allyAttack0_x);
        // sensor.AddObservation(allyAttack0_z);
        // sensor.AddObservation(allyAttack1_x);
        // sensor.AddObservation(allyAttack1_z);
        // sensor.AddObservation(allyAttack2_x);
        // sensor.AddObservation(allyAttack2_z);

        // // Midfield Rod
        // sensor.AddObservation(allyMidfield_x);
        // sensor.AddObservation(allyMidfield_z);
        // sensor.AddObservation(allyMidfield_Rot_z);
        // // Midfield Rod Players
        // sensor.AddObservation(allyMidfield0_x);
        // sensor.AddObservation(allyMidfield0_z);
        // sensor.AddObservation(allyMidfield1_x);
        // sensor.AddObservation(allyMidfield1_z);
        // sensor.AddObservation(allyMidfield2_x);
        // sensor.AddObservation(allyMidfield2_z);
        // sensor.AddObservation(allyMidfield3_x);
        // sensor.AddObservation(allyMidfield3_z);
        // sensor.AddObservation(allyMidfield4_x);
        // sensor.AddObservation(allyMidfield4_z);

        // // Defence Rod
        // sensor.AddObservation(allyDefence_x);
        // sensor.AddObservation(allyDefence_z);
        // sensor.AddObservation(allyDefence_Rot_z);
        // // Defence Rod Players
        // sensor.AddObservation(allyDefence0_x);
        // sensor.AddObservation(allyDefence0_z);
        // sensor.AddObservation(allyDefence1_x);
        // sensor.AddObservation(allyDefence1_z);

        // // Goalkeeper Rod
        // sensor.AddObservation(allyGoalkeeper_x);
        // sensor.AddObservation(allyGoalkeeper_z);
        // sensor.AddObservation(allyGoalkeeper_Rot_z);
        // // GoalKeeper Rod Players 
        // sensor.AddObservation(allyGoalkeeper0_x);
        // sensor.AddObservation(allyGoalkeeper0_z);
        // sensor.AddObservation(allyGoalkeeper1_x);
        // sensor.AddObservation(allyGoalkeeper1_z);
        // sensor.AddObservation(allyGoalkeeper2_x);
        // sensor.AddObservation(allyGoalkeeper2_z);

        // sensor.AddObservation(allyGoal_x);
        // sensor.AddObservation(allyGoal_z);

        // sensor.AddObservation(enemyGoal_x);
        // sensor.AddObservation(enemyGoal_z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        currentScore[0] = DI.robot_score;
        currentScore[1] = DI.player_score;

        allyAttackLinearCommand = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        allyAttackRotationCommand = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        allyMidfieldLinearCommand = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        allyMidfieldRotationCommand = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);        
        allyDefenceLinearCommand = Mathf.Clamp(actionBuffers.ContinuousActions[4], -1f, 1f);
        allyDefenceRotationCommand = Mathf.Clamp(actionBuffers.ContinuousActions[5], -1f, 1f);
        allyGoalkeeperLinearCommand = Mathf.Clamp(actionBuffers.ContinuousActions[6], -1f, 1f);
        allyGoalkeeperRotationCommand = Mathf.Clamp(actionBuffers.ContinuousActions[7], -1f, 1f);
        
        actions[0] = allyAttackLinearCommand;
        actions[1] = allyAttackRotationCommand;
        actions[2] = allyMidfieldLinearCommand;
        actions[3] = allyMidfieldRotationCommand;
        actions[4] = allyDefenceLinearCommand;
        actions[5] = allyDefenceRotationCommand;
        actions[6] = allyGoalkeeperLinearCommand;
        actions[7] = allyGoalkeeperRotationCommand;


        // Example reward structure here based on physical table information
        if (prevScore[0] != currentScore[0] || prevScore[1] != currentScore[1])
        {   
            if (currentScore[0] > prevScore[0])
            {
                AddReward(1f);
                EndEpisode();
            }

            if (currentScore[1] > prevScore[1]){
                AddReward(-1f);
                EndEpisode();
            }
        }

        prevScore[0] = currentScore[0];
        prevScore[1] = currentScore[1];
    }    

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
