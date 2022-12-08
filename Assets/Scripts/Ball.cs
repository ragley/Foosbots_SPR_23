using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public PlayerColor inGoalColor;
    public TrackKicks isKick;
    Vector3 ballReset;
    public Rigidbody rBody;
    private float kickForce;
    Vector3 kickVector;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        kickForce = 200f;
        kickVector = new Vector3(0f, 0f, 0f);
    }

    public void Reset(float resetX, float resetZ)
    {
        rBody.velocity = new Vector3(0f, 0f, 0f);
        rBody.angularVelocity = new Vector3(0f, 0f, 0f);

        inGoalColor = PlayerColor.none;

        ballReset = new Vector3(resetX, 0.0029778f, resetZ);
        gameObject.transform.localPosition = ballReset;
    }

    public void AutoKick(float xInput, float zInput)
    {
        kickVector.x = xInput;
        kickVector.z = zInput;
        kickVector *= kickForce;
        rBody.AddForce(kickVector);
        Debug.Log(kickVector);
    }

}
