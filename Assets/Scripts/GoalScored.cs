using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : MonoBehaviour
{
    public PlayerColor GoalColor;
    public GameObject collisionTarget;

    Rigidbody objectBody;

    public void Start()
    {
        objectBody = collisionTarget.GetComponent<Rigidbody>();
    }

    public void OnTriggerEnter(Collider collisionData)
    {

        if (collisionData.gameObject.tag == "Ball")
        {
            objectBody.velocity = new Vector3(0f, 0f, 0f);
            objectBody.angularVelocity = new Vector3(0f, 0f, 0f);
            collisionData.gameObject.GetComponent<Ball>().inGoalColor = GoalColor;
        }

    }
}
