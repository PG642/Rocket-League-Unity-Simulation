using UnityEngine;

public class Resettable : MonoBehaviour
{
    public Vector3 _position;
    public Vector3 _velocity;
    public Quaternion _rotation;
    public Vector3 _angularVelocity;
    protected Rigidbody _rb;

    public void CancelUnityImpulse()
    {
        _rb.position = _position;
        _rb.velocity = _velocity;
        _rb.angularVelocity = _angularVelocity;
        _rb.rotation = _rotation;
    }

    private void FixedUpdate()
    {
        Clone(_rb);
    }

    private void Clone(Rigidbody rb)
    {
        _position = rb.position;

        _position = new Vector3
        {
            x = _position.x,
            y = _position.y,
            z = _position.z,
        };
        _rotation = new Quaternion((_rotation = rb.rotation).x, _rotation.y, _rotation.z, _rotation.w);
        _angularVelocity = new Vector3(
            _angularVelocity.x,
            _angularVelocity.y,
            _angularVelocity.z);
        _velocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
    }
}