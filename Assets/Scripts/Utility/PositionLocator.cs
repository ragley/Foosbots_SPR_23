using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionLocator : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {
        Debug.Log("Global:");
        Debug.Log(gameObject.transform.position);
        Debug.Log("Local:");
        Debug.Log(gameObject.transform.localPosition);
    }

    // Update is called once per frame
    void Update()
    {
        /*        ball.reset();
                Debug.Log("Global:");
                Debug.Log(gameObject.transform.position);
                Debug.Log("Local:");
                Debug.Log(gameObject.transform.localPosition);*/
    }
}
