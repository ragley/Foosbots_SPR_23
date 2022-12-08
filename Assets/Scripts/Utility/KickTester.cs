using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickTester : MonoBehaviour
{
    public Rigidbody rBody;
    Vector3 kickVector;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        kickVector = new Vector3(0f, 0f, 0f);
        kickVector.x = 0f;
        kickVector.z = -1000f;
        rBody.AddForce(kickVector);
        Debug.Log(kickVector);
    }

}
