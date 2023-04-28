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
    public float hitAngularVelocity;
    Vector3 kickVector;
    Vector3 ballResetPos;

    public enum CollisionRod {Attack, Midfield, Defence, Goalkeeper, none};
    public CollisionRod collision_rod;

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
        collision_rod = CollisionRod.none;
        hitAngularVelocity = 0f;
        kicked = false;
        
        // Reset the position
        gameObject.transform.localPosition = ballResetPos;
        
        //print("Ball Reset");     
        AutoKick();
    }
    public void ResetFull(float xPos, float zPos)
    {
        ballResetPos = new Vector3(xPos, 0.0029778f, zPos);

        // Set Velocities to 0
        ballRB.velocity = new Vector3(0f, 0f, 0f);
        ballRB.angularVelocity = new Vector3(0f, 0f, 0f);
        
        // Set goal and last kicked Color to none
        inGoalColor = PlayerColor.none;
        lastKickedColor = PlayerColor.none;
        collision_rod = CollisionRod.none;
        hitAngularVelocity = 0f;
        kicked = false;
        
        // Reset the position
        gameObject.transform.localPosition = ballResetPos;

        AutoKick();
    }

    public void OnCollisionEnter(Collision collisionData)
    {

        string ballLastHit = collisionData.gameObject.tag;
        if (ballLastHit == "BluePlayer")
        { 
            //print("=========KICKED=========== you whore: " + ballLastHit);
            lastKickedColor = PlayerColor.blue;
            kicked = true;
            //print(collisionData.gameObject.name);
            if (collisionData.gameObject.name.Equals("Team.Blue.Attack"))
            {
                collision_rod = CollisionRod.Attack;

            } else if (collisionData.gameObject.name.Equals("Team.Blue.Defence"))
            {
                collision_rod = CollisionRod.Defence;

            } else if (collisionData.gameObject.name.Equals("Team.Blue.Midfield"))
            {
                collision_rod = CollisionRod.Midfield;

            } else if (collisionData.gameObject.name.Equals("Team.Blue.Goalkeeper"))
            {
                collision_rod = CollisionRod.Goalkeeper;

            } else
            {
                collision_rod = CollisionRod.none;
            }
            
            hitAngularVelocity = getHitAngularVelocity(collisionData);
            // Use collisionRod enum and tags on rods to get which rod hit the ball, and the angular velocity of that rod at that time
            // no reward for hitting towards the ally goal, positive reward hitting toward the enemy goal (ex CW vs CCW)
            // keep reward for hitting the ball, just make it smaller that it was before (2) -> (1)
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

    public float getHitAngularVelocity(Collision collisionData)
    { 
        Rigidbody collisionRB = collisionData.gameObject.GetComponent<Rigidbody>();
        return collisionRB.angularVelocity.z;
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
