using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody body;

    private float fwdbackInput;
    private float horizontalInput;

private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        fwdbackInput = Input.GetAxis("Vertical");

        body.velocity = new Vector3(horizontalInput * speed, fwdbackInput * speed);

    }
}
