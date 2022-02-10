using System;
using UnityEngine;

public class SuspensionCollider : MonoBehaviour
{
    const float RaycastOffset = 0.01f;
    const float PenetrationTolerance = 0.001f;

    private int _lastFrameCollision;
    private float _maxAcceleration;
    private MeshCollider _meshCollider;
    private WheelSuspension _wheelSuspension;
    private Rigidbody _rb;

    public bool disableSuspension;
    public float contactDepth; //How much the suspension is compressed/extended
    public float lastContactDepth;
    public float stiffness;
    public float damper;


    public SuspensionCollider()
    {
        _lastFrameCollision = 0;
    }


    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _wheelSuspension = GetComponentInParent<WheelSuspension>();
        _rb = GetComponentInParent<Rigidbody>();
        stiffness = _wheelSuspension.stiffness;
        damper = _wheelSuspension.damper;
        disableSuspension = false;
    }

    public void CalculateContactDepth(Collider other)
    {
        if (disableSuspension) return;

        lastContactDepth = contactDepth;
        contactDepth = -_wheelSuspension.extensionDistance;
        transform.localPosition = new Vector3(0, contactDepth, 0);
        if (other == null) return;

        bool significantOverlap;
        RayCastCollision();
        do
        {
            significantOverlap = PenetrativeCollision(other);
        } while (significantOverlap);

        if (!(contactDepth > 0)) return;

        var speed = (contactDepth - lastContactDepth) / Time.fixedDeltaTime;
        var springAcceleration = Math.Min(contactDepth * stiffness + speed * damper, 6f);
        if (_maxAcceleration < springAcceleration)
        {
            _maxAcceleration = springAcceleration;
        }

        var worldSpringAcceleration = transform.TransformVector(new Vector3(springAcceleration, 0.0f, 0.0f));
        _rb.AddForceAtPosition(worldSpringAcceleration, _wheelSuspension.displacementCollider.transform.position,
            ForceMode.Acceleration);
    }

    //Calculate Collision with a Physics.ComputePenetration and transforms the wheel in y-Direction.
    private bool PenetrativeCollision(Collider other)
    {
        var ownBodyTransform = _meshCollider.transform;
        var collisionBodyTransform = other.transform;

        var isOverlap = Physics.ComputePenetration(_meshCollider, ownBodyTransform.position,
            ownBodyTransform.rotation,
            other, collisionBodyTransform.position, collisionBodyTransform.rotation, out var direction,
            out var distance);
        if (!isOverlap) return false;

        var penetration = Vector3.Dot(direction * distance, _meshCollider.transform.right);
        var isSignificantOverlap = MoveWheel(penetration);

        return isSignificantOverlap;
    }

    //Approximate Collision with RayCast and transforms the wheel in y-Direction.
    private void RayCastCollision()
    {
        var hit = Physics.Raycast(
            origin: _wheelSuspension.displacementCollider.transform.position +
                    _wheelSuspension.displacementCollider.transform.TransformDirection(new Vector3(0.0f,
                        RaycastOffset - _wheelSuspension.radius, 0.0f)),
            direction: _wheelSuspension.displacementCollider.transform.TransformDirection(
                new Vector3(0.0f, -1.0f, 0.0f)),
            maxDistance: _wheelSuspension.extensionDistance + _wheelSuspension.compressionDistance + RaycastOffset,
            hitInfo: out var hitRay);
        if (!hit) return;

        var calculatedContactDepth = _wheelSuspension.compressionDistance - (hitRay.distance - RaycastOffset);
        MoveWheelContactDepth(calculatedContactDepth);
    }

    private bool MoveWheel(float penetration)
    {
        contactDepth = Math.Min(contactDepth + penetration, _wheelSuspension.compressionDistance);
        _meshCollider.transform.localPosition = new Vector3(0, contactDepth, 0);
        var significantOverlap =
            penetration > PenetrationTolerance && _wheelSuspension.compressionDistance > contactDepth;
        return significantOverlap;
    }

    private void MoveWheelContactDepth(float calculatedContactDepth)
    {
        contactDepth = calculatedContactDepth;
        _meshCollider.transform.localPosition = new Vector3(0, this.contactDepth, 0);
    }
}