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

    public float contactDepth;//How much the suspension is compressed/extended
    public float lastcontactDepth;
    private Rigidbody _rb;
    public float stiffnes;
    const float RaycastOffset = 0.01f;
    const float PenetrationTolerance = 0.001f;

    public SuspensionCollider()
    {
        _lastFrameCollision = 0;
    }


    private void Start()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _wheelSuspension = GetComponentInParent<WheelSuspension>();
        _rb = GetComponentInParent<Rigidbody>();
        stiffnes = _wheelSuspension.isFrontWheel ? 328.0044072f : 330.6491205f;
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

        bool significantOverlap = false;
        RayCastCollision();
        do {
            significantOverlap = PenetrativeCollision(other);
        } while (significantOverlap);

        if (contactDepth > 0)
        {
            float damper = 20f;
            float speed = (contactDepth -lastcontactDepth ) / Time.fixedDeltaTime;
            var springAcceleration = contactDepth * stiffnes + speed * damper;
            var worldSpringAcceleration = transform.TransformVector(new Vector3(springAcceleration, 0.0f, 0.0f));
            Debug.Log(transform.parent.parent.name + " " + Time.frameCount + " " + springAcceleration);
            _rb.AddForceAtPosition(worldSpringAcceleration, _wheelSuspension.displacementCollider.transform.position,
                ForceMode.Acceleration);
        }
    }

    public bool PenetrativeCollision(Collider other)
    {
        var ownBodyTransform = _meshCollider.transform;
        var collisionBodyTransform = other.transform;
        var significantOverlap = Physics.ComputePenetration(_meshCollider, ownBodyTransform.position,
            ownBodyTransform.rotation,
            other, collisionBodyTransform.position, collisionBodyTransform.rotation, out Vector3 direction,
            out float distance);
        if (significantOverlap)
        {
            //Debug.Log(transform.parent.parent.name + " " + Time.frameCount);

            float penetration = Vector3.Dot(direction * distance, _meshCollider.transform.right);
            significantOverlap = MoveWheel(penetration);
        }

        return significantOverlap;
    }

    private bool RayCastCollision()
    {
        var hit = Physics.Raycast(origin: _wheelSuspension.displacementCollider.transform.position +   _wheelSuspension.displacementCollider.transform.TransformDirection(new Vector3(0.0f,RaycastOffset-_wheelSuspension.radius, 0.0f)),
            direction: _wheelSuspension.displacementCollider.transform.TransformDirection(
                new Vector3(0.0f, -1.0f, 0.0f)),
            maxDistance: _wheelSuspension.extensionDistance + _wheelSuspension.compressionDistance + RaycastOffset, hitInfo: out var hitRay);
        if (hit)
        {
            var contactDepth = _wheelSuspension.compressionDistance - (hitRay.distance - RaycastOffset);
            MoveWheelContactDepth(contactDepth);
        }

        return hit;
    }

    private bool MoveWheel(float penetration)
    {
        contactDepth = Math.Min(contactDepth + penetration, _wheelSuspension.compressionDistance);
        _meshCollider.transform.localPosition = new Vector3(0, contactDepth, 0);
        bool significantOverlap = penetration > PenetrationTolerance && _wheelSuspension.compressionDistance > contactDepth;
        return significantOverlap;
    }
    
    private void MoveWheelContactDepth(float contactDepth)
    {
        this.contactDepth = contactDepth;
        _meshCollider.transform.localPosition = new Vector3(0, this.contactDepth, 0);
    }
}