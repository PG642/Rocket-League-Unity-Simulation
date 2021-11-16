using UnityEngine;

public class Resettable : MonoBehaviour
{
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
    public Vector3 angularVelocity;
    protected Rigidbody rb;

    public void CancelUnityImpulse()
    {
        rb.position = position;
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
        rb.rotation = rotation;
    }

    private void FixedUpdate()
    {
        Clone(rb);
    }

    private void Clone(Rigidbody rbToClone)
    {
        position = rbToClone.position;

        position = new Vector3
        {
            x = position.x,
            y = position.y,
            z = position.z,
        };
        rotation = new Quaternion((rotation = rbToClone.rotation).x, rotation.y, rotation.z, rotation.w);
        angularVelocity = new Vector3(
            angularVelocity.x,
            angularVelocity.y,
            angularVelocity.z);
        velocity = new Vector3(velocity.x, velocity.y, velocity.z);
    }
}