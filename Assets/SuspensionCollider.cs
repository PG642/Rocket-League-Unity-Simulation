using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class SuspensionCollider : MonoBehaviour
{
    private MeshCollider _meshCollider;
    private int _lastFrameCollision;
    private WheelSuspension _wheelSuspension;


    public float contactDepth; //How much the suspension is compressed/extended
    public float lastcontactDepth;
    private Rigidbody _rb;
    public float stiffnes;
    const float RaycastOffset = 0.01f;
    const float PenetrationTolerance = 0.001f;
    public float damper;
    private float maxAcceleration = 0.0f;


    public SuspensionCollider()
    {
        _lastFrameCollision = 0;
    }


    private void Awake()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _wheelSuspension = GetComponentInParent<WheelSuspension>();
        _rb = GetComponentInParent<Rigidbody>();
        stiffnes = _wheelSuspension.stiffness;
        damper = _wheelSuspension.damper;
    }

    public void CalculateContactdepth(Collider other)
    {
        lastcontactDepth = contactDepth;
        contactDepth = -_wheelSuspension.extensionDistance;
        transform.localPosition = new Vector3(0, contactDepth, 0);
        if (other == null)
        {
            return;
        }

        bool significantOverlap;
        RayCastCollision();
        do
        {
            significantOverlap = PenetrativeCollision(other);
        } while (significantOverlap);

        if (contactDepth > 0)
        {
            
            float speed = (contactDepth - lastcontactDepth) / Time.fixedDeltaTime;
            var springAcceleration = Math.Min(contactDepth * stiffnes + speed * damper,6f);
            // var springAcceleration = contactDepth * stiffnes + speed * damper;
            if (maxAcceleration < springAcceleration)
            {
                maxAcceleration = springAcceleration;
            }
            // Debug.Log($"Speed:{speed} --- ConatactDepth:{contactDepth} --- MaxAxx:{springAcceleration} timeFrame:{Time.frameCount} Wheel:{_wheelSuspension.name}");

            var worldSpringAcceleration = transform.TransformVector(new Vector3(springAcceleration, 0.0f, 0.0f));
            _rb.AddForceAtPosition(worldSpringAcceleration, _wheelSuspension.displacementCollider.transform.position,
                ForceMode.Acceleration);
        }
    }
    //Calculate Collision with a Physics.ComputePenetration and transforms the wheel in y-Direction.
    private bool PenetrativeCollision(Collider other)
    {
        var ownBodyTransform = _meshCollider.transform;
        var collisionBodyTransform = other.transform;
        var isSignificantOverlap = false;
        var isOverlap = Physics.ComputePenetration(_meshCollider, ownBodyTransform.position,
            ownBodyTransform.rotation,
            other, collisionBodyTransform.position, collisionBodyTransform.rotation, out Vector3 direction,
            out float distance);
        if (isOverlap)
        {
            float penetration = Vector3.Dot(direction * distance, _meshCollider.transform.right);
            isSignificantOverlap = MoveWheel(penetration);
        }

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
        if (hit)
        {
            var contactDepth = _wheelSuspension.compressionDistance - (hitRay.distance - RaycastOffset);
            MoveWheelContactDepth(contactDepth);
        }
    }

    private bool MoveWheel(float penetration)
    {
        contactDepth = Math.Min(contactDepth + penetration, _wheelSuspension.compressionDistance);
        _meshCollider.transform.localPosition = new Vector3(0, contactDepth, 0);
        bool significantOverlap =
            penetration > PenetrationTolerance && _wheelSuspension.compressionDistance > contactDepth;
        return significantOverlap;
    }

    private void MoveWheelContactDepth(float contactDepth)
    {
        this.contactDepth = contactDepth;
        _meshCollider.transform.localPosition = new Vector3(0, this.contactDepth, 0);
    }
}