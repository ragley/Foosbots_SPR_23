using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBouncePhysics : MonoBehaviour
{

    public Rigidbody ballBody;

    private float kickForce;
    Vector3 kickVector;


    void Start()
    {
        kickVector = new Vector3(0f, 0f, 0f);
        kickForce = 50;

    }

    public void OnCollisionEnter(Collision collisionData)
    {
        if (collisionData.gameObject.tag == "Ball")
        {
            kick(collisionData.contacts[0].normal);
        }

    }

    void kick(Vector3 inputVector)
    {
        kickVector.x = kickForce * inputVector.x;
        kickVector.z = kickForce * inputVector.z;
        kickVector *= -1;
        ballBody.AddForce(kickVector);

    }
}
