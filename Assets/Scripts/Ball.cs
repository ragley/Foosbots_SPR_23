using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public PlayerColor inGoalColor;
    public PlayerColor lastKickedColor;
    public TrackKicks isKick;
    public Rigidbody ballRB;
    public float autoKickStrength;
    public float maxAngularVelocity;
    public bool kicked = false;
    Vector3 kickVector;
    Vector3 ballResetPos;

    void Start()
    {
        ballRB = GetComponent<Rigidbody>();
        ballRB.maxAngularVelocity = maxAngularVelocity;
    }

    public void Reset(bool randomPos = true)
    {
        float xPos = 0f;
        float zPos = 0f;
        

        if (randomPos) {
            // Random Position across the width of the field
            zPos = Random.Range(-0.00298f, 0.00298f);
        }

        ballResetPos = new Vector3(xPos, 0.0029778f, zPos);

        // Set Velocities to 0
        ballRB.velocity = new Vector3(0f, 0f, 0f);
        ballRB.angularVelocity = new Vector3(0f, 0f, 0f);
        
        // Set goal and last kicked Color to none
        inGoalColor = PlayerColor.none;
        lastKickedColor = PlayerColor.none;
        kicked = false;
        
        // Reset the position
        gameObject.transform.localPosition = ballResetPos;
        
        //print("Ball Reset");     
    }

    public void OnCollisionEnter(Collision collisionData)
    {

        string ballLastHit = collisionData.gameObject.tag;
        if (ballLastHit == "BluePlayer")
        { 
            //print("=========KICKED=========== you whore: " + ballLastHit);
            lastKickedColor = PlayerColor.blue;
            kicked = true;
        }
        else if (ballLastHit == "RedPlayer")
        {
            //print("=========KICKED=========== you whore: " + ballLastHit);
            lastKickedColor = PlayerColor.red;
            kicked = true;
        }
        else
        {
            kicked = false;
            //print("=========Hit wall======== ur bad");
        }
   }

   public void OnCollisionExit(Collision collisionData)
   {

   }

    public void AutoKick()
    {   
        kickVector = new Vector3(Random.Range(-1f,1f), 0f, Random.Range(-1f,1f));
        kickVector *= autoKickStrength;

        ballRB.AddForce(kickVector, ForceMode.VelocityChange);
        lastKickedColor = PlayerColor.none;
        //Debug.Log(kickVector);
    }
    

}
