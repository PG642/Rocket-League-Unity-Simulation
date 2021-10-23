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
    private Rigidbody _rb;
    public float stiffnes;
    public float tolerance = 0.01f;

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
        contactDepth = -_wheelSuspension.extensionDistance;
        transform.localPosition = new Vector3(0, contactDepth, 0);
        if (other == null)
        {
            return;
        }

        bool significantOverlap;
        do
        {
            // significantOverlap = PenetrativeCollision(other);
            significantOverlap = false;
            RayCastCollision();
        } while (significantOverlap);

        if (contactDepth > 0)
        {
            var springAcceleration = contactDepth * stiffnes;
            var worldSpringAcceleration = transform.TransformVector(new Vector3(springAcceleration, 0.0f, 0.0f));
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
            Debug.Log(transform.parent.parent.name + " " + Time.frameCount);

            float penetration = Vector3.Dot(direction * distance, _meshCollider.transform.right);
            significantOverlap = MoveWheel(penetration);
        }

        return significantOverlap;
    }

    private bool RayCastCollision()
    {
        var hit = Physics.Raycast(origin: _wheelSuspension.displacementCollider.transform.position +   _wheelSuspension.displacementCollider.transform.TransformDirection(new Vector3(0.0f,tolerance-_wheelSuspension.radius, 0.0f)),
            direction: _wheelSuspension.displacementCollider.transform.TransformDirection(
                new Vector3(0.0f, -1.0f, 0.0f)),
            maxDistance: _wheelSuspension.extensionDistance + _wheelSuspension.compressionDistance + tolerance, hitInfo: out var hitRay);
        if (hit)
        {
            Debug.Log("hit");
            var contactDepth = _wheelSuspension.compressionDistance - (hitRay.distance - tolerance);
            MoveWheelContactDepth(contactDepth);
        }

        return hit;
    }

    private bool MoveWheel(float penetration)
    {
        bool significantOverlap;
        contactDepth = Math.Min(contactDepth + penetration, _wheelSuspension.compressionDistance);
        _meshCollider.transform.localPosition = new Vector3(0, contactDepth, 0);
        significantOverlap = penetration > 0.0001f && _wheelSuspension.compressionDistance > contactDepth;
        return significantOverlap;
    }
    
    private void MoveWheelContactDepth(float contactDepth)
    {
        this.contactDepth = contactDepth;
        _meshCollider.transform.localPosition = new Vector3(0, this.contactDepth, 0);
    }
}