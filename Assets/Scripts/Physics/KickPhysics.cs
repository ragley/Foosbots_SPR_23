using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickPhysics : MonoBehaviour
{

    public Rigidbody ballBody;
    public Rigidbody rodBody;

    private float kickForce;
    Vector3 kickVector;
    private TrackKicks yes;
    private TrackKicks no;


    void Start()
    {
        kickVector = new Vector3(0f, 0f, 0f);
        kickForce = 10;
        yes = TrackKicks.yesKick;
        no = TrackKicks.noKick;

    }

    public void OnCollisionEnter(Collision collisionData)
    {
        if (collisionData.gameObject.tag == "Ball")
        {
            kick(collisionData.contacts[0].normal);
/*            collisionData.gameObject.GetComponent<Ball>().isKick = yes;*/
        }

    }


    void kick(Vector3 inputVector)
    {
        kickVector.x = kickForce * inputVector.x * rodBody.angularVelocity.z;
        kickVector.z = kickForce * inputVector.z * rodBody.angularVelocity.z;
        /*        kickVector *= -1;*/
        ballBody.AddForce(kickVector, ForceMode.Impulse);

    }
}
