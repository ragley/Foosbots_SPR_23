using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorqueTester : MonoBehaviour
{
    Rigidbody rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = gameObject.GetComponent<Rigidbody>();
        rBody.AddTorque(0f, 0f, -1000f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
