using System;
using UnityEngine;

public class Resettable : MonoBehaviour
{
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 inertiaTensor;
    private Quaternion rotation;
    private Vector3 angularVelocity;
    private Quaternion inertiaTensorRotation;
    protected Rigidbody rb;


    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void CancelUnityImpulse()
    {
        rb.position = position;
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
        rb.rotation = rotation;
        rb.inertiaTensor = inertiaTensor;
        rb.inertiaTensorRotation = inertiaTensorRotation;
    }

    public virtual void FixedUpdate()
    {
        Clone(rb);
    }

    protected void Clone(Rigidbody rbToClone)
    {
        var rbPosition = rbToClone.position;
        position = new Vector3
        {
            x = rbPosition.x,
            y = rbPosition.y,
            z = rbPosition.z,
        };
        var rbRotation = rbToClone.rotation;
        rotation = new Quaternion(rbRotation.x, rbRotation.y, rbRotation.z, rbRotation.w);
        var rbAngularVelocity = rbToClone.angularVelocity;
        angularVelocity = new Vector3(
            rbAngularVelocity.x,
            rbAngularVelocity.y,
            rbAngularVelocity.z);
        var rbVelocity = rbToClone.velocity;
        velocity = new Vector3(rbVelocity.x, rbVelocity.y, rbVelocity.z);
        var rbInertiaTensor = rbToClone.inertiaTensor;
        inertiaTensor = new Vector3(
            rbInertiaTensor.x,
            rbInertiaTensor.y,
            rbInertiaTensor.z);
        var rbInertiaTensorRotation = rbToClone.inertiaTensorRotation;
        inertiaTensorRotation = new Quaternion(rbInertiaTensorRotation.x, rbInertiaTensorRotation.y, rbInertiaTensorRotation.z, rbInertiaTensorRotation.w);
    }

    public void ResetValues()
    {
        Clone(rb);
    }
}