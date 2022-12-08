using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatLogger : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rBody;
    void Start()
    {
        rBody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 Log = new Vector3(rBody.angularVelocity.x, rBody.angularVelocity.y, rBody.angularVelocity.z);
        Debug.Log(Log);
    }
}
