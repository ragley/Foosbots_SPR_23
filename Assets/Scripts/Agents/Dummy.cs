using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dummy : MonoBehaviour
{
    public GameObject allyAttack;
    public GameObject allyDefence;
    public GameObject allyGoalkeeper;
    public GameObject allyMidfield;

    public float Force;
    public float Torque;

    Rigidbody attackRod;
    Rigidbody defenceRod;
    Rigidbody goalkeeperRod;
    Rigidbody midfieldRod;
    Rigidbody ballBody;

    Vector3 attackControlForce = Vector3.zero;
    Vector3 attackControlTorque = Vector3.zero;
    Vector3 defenceControlForce = Vector3.zero;
    Vector3 defenceControlTorque = Vector3.zero;
    Vector3 goalkeeperControlForce = Vector3.zero;
    Vector3 goalkeeperControlTorque = Vector3.zero;
    Vector3 midfieldControlForce = Vector3.zero;
    Vector3 midfieldControlTorque = Vector3.zero;


    void Start()
    {
        attackRod = allyAttack.gameObject.GetComponent<Rigidbody>();
        defenceRod = allyDefence.gameObject.GetComponent<Rigidbody>();
        goalkeeperRod = allyGoalkeeper.gameObject.GetComponent<Rigidbody>();
        midfieldRod = allyMidfield.gameObject.GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        attackControlForce = new Vector3(0f, 0f, Force * Random.Range(-1f, 1f));
        attackControlTorque = new Vector3(0f, 0f, Torque * Random.Range(-1f, 1f));
        defenceControlForce = new Vector3(0f, 0f, Force * Random.Range(-1f, 1f));
        defenceControlTorque = new Vector3(0f, 0f, Torque * Random.Range(-1f, 1f));
        goalkeeperControlForce = new Vector3(0f, 0f, Force * Random.Range(-1f, 1f));
        goalkeeperControlTorque = new Vector3(0f, 0f, Torque * Random.Range(-1f, 1f));
        midfieldControlForce = new Vector3(0f, 0f, Force * Random.Range(-1f, 1f));
        midfieldControlTorque = new Vector3(0f, 0f, Torque * Random.Range(-1f, 1f));

        attackRod.AddForce(attackControlForce);
        attackRod.AddTorque(attackControlTorque);
        defenceRod.AddForce(defenceControlForce);
        defenceRod.AddTorque(defenceControlTorque);
        goalkeeperRod.AddForce(goalkeeperControlForce);
        goalkeeperRod.AddTorque(goalkeeperControlTorque);
        midfieldRod.AddForce(midfieldControlForce);
        midfieldRod.AddTorque(midfieldControlTorque);
    }
}
