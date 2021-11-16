using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviousCarState : MonoBehaviour
{
    private Rigidbody rb;

    public Vector3 velocity { get; private set; }
    public Vector3 position { get; private set; }
    public Vector3 eulerAngles { get; private set; }
    public Vector3 angularVelocity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        velocity = rb.velocity;
        position = transform.position;
        eulerAngles = transform.eulerAngles;
        angularVelocity = rb.angularVelocity;
    }
}
