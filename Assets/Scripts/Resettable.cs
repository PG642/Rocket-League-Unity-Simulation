using UnityEngine;

public class Resettable : MonoBehaviour
{
    public float friction = 2f;
    public Vector3 position;
    public Vector3 velocity;
    private Vector3 inertiaTensor;
    public Quaternion rotation;
    public Vector3 angularVelocity;
    public Quaternion inertiaTensorRotation;
    protected Rigidbody rb;

    public void CancelUnityImpulse()
    {
        rb.position = position;
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
        rb.rotation = rotation;
        rb.inertiaTensor = inertiaTensor;
        rb.inertiaTensorRotation = inertiaTensorRotation;
    }

    private void FixedUpdate()
    {
        Clone(rb);
    }

    private void Clone(Rigidbody rbToClone)
    {
        position = new Vector3
        {
            x = rbToClone.position.x,
            y = rbToClone.position.y,
            z = rbToClone.position.z,
        };
        rotation = new Quaternion(rbToClone.rotation.x, rbToClone.rotation.y, rbToClone.rotation.z, rbToClone.rotation.w);
        angularVelocity = new Vector3(
            rbToClone.angularVelocity.x,
            rbToClone.angularVelocity.y,
            rbToClone.angularVelocity.z);
        velocity = new Vector3(rbToClone.velocity.x, rbToClone.velocity.y, rbToClone.velocity.z);
        inertiaTensor = new Vector3(
            rbToClone.inertiaTensor.x,
            rbToClone.inertiaTensor.y,
            rbToClone.inertiaTensor.z);
        inertiaTensorRotation = new Quaternion(rbToClone.inertiaTensorRotation.x, rbToClone.inertiaTensorRotation.y, rbToClone.inertiaTensorRotation.z, rbToClone.inertiaTensorRotation.w);
    }
}