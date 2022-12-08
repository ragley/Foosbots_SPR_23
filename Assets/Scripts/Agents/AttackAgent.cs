using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AttackAgent : Agent
{
    public Ball ball;
    public PlayerColor allyColor;
    public GameObject rod;
    public GameObject enemyGoal;
    public GameObject man1;
    public GameObject man2;
    public GameObject man3;
    public GameObject enemyDefenceRod;

    Rigidbody rodBody;
    Vector3 initialKick;

    int counter;

    void Start()
    {
        counter = 0;

        rodBody = rod.GetComponent<Rigidbody>();

        initialKick = Vector3.zero;
    }

    public override void OnEpisodeBegin()
    {
        counter = 0;

        rodBody.velocity = new Vector3(0f, 0f, 0f);
        rodBody.angularVelocity = new Vector3(0f, 0f, 0f);
        rod.transform.localPosition = new Vector3(-0.002214001f, 0.003497f, 0f);
        rod.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);

        /*        ball.rBody.velocity = new Vector3(0f, 0f, 0f);
                ball.rBody.angularVelocity = new Vector3(0f, 0f, 0f);
                ball.transform.localPosition = new Vector3(Random.Range(-0.002694f, -0.00536f), 0.0029778f, Random.Range(-0.002861f, 0.002861f));*/

        ball.Reset(Random.Range(-0.002694f, -0.00536f), Random.Range(-0.002861f, 0.002861f));

        initialKick.z = Random.Range(0f, 125f);
        initialKick.x = Random.Range(0f, 125f);

        ball.rBody.AddForce(initialKick);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // ball information: 4 observations
        sensor.AddObservation(ball.transform.position.x);
        sensor.AddObservation(ball.transform.position.z);
        sensor.AddObservation(ball.rBody.velocity.x);
        sensor.AddObservation(ball.rBody.velocity.z);

        // rod information: 3 observations
        sensor.AddObservation(rod.transform.position.x);
        sensor.AddObservation(rod.transform.position.z);
        sensor.AddObservation(rod.transform.localRotation.z);
        // foosmen information 6 observations
        sensor.AddObservation(man1.transform.position.x);
        sensor.AddObservation(man1.transform.position.z);
        sensor.AddObservation(man2.transform.position.x);
        sensor.AddObservation(man2.transform.position.z);
        sensor.AddObservation(man3.transform.position.x);
        sensor.AddObservation(man3.transform.position.z);

        // goal information: 2 observations
        sensor.AddObservation(enemyGoal.transform.position.x);
        sensor.AddObservation(enemyGoal.transform.position.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // torque and force control
        Vector3 controlAttackForce = Vector3.zero;
        Vector3 controlAttackTorque = Vector3.zero;
        controlAttackForce.z = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        controlAttackTorque.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        rodBody.AddForce(controlAttackForce);
        rodBody.AddTorque(controlAttackTorque);

        // rewards
        if (ball.inGoalColor != allyColor && ball.inGoalColor != PlayerColor.none)
        {
            AddReward(1f);
            EndEpisode();
        }
        else if (ball.inGoalColor == allyColor)
        {
            AddReward(-1f);
            EndEpisode();
        }
        else
        {
            // reward/penalize based on zone
            if (ball.transform.position.x < enemyDefenceRod.transform.position.x)
            {
                AddReward(0.00005f);
            }
            else
            {
                AddReward(-0.00005f);
            }
            // reward kick
            if (ball.isKick == TrackKicks.yesKick)
            {
                AddReward(0.005f);
                ball.isKick = TrackKicks.noKick;
            }
            // increment end step counter
            counter++;
            if (counter >= 2500)
            {
                /*                ball.AutoKick(Random.Range(-1f, 1f), Random.Range(-1, 1f));
                                counter = 0;*/
                EndEpisode();
            }
        }


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}
