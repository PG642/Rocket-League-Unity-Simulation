using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class SuspensionCollider : MonoBehaviour
{
    public bool hasContact;

    private MeshCollider _meshCollider;
    private Vector3 _localPosition;
    private int _lastFrameCollision;
    private WheelSuspension _wheelSuspension;

    public float sprungMass; //How much weight the wheel has to support
    public float contactDepth; //How much the suspension is compressed/extended
    public float contactSpeed; //How fast the suspension moves
    public Vector3 contactVelocity; //Velocity Vector of colliding object
    public float suspensionForce; //How much force the suspension applies to the car

    public SuspensionCollider()
    {
        _lastFrameCollision = 0;
    }


    private void Start()
    {
        _meshCollider = GetComponent<MeshCollider>();
        _wheelSuspension = GetComponentInParent<WheelSuspension>();
        _localPosition = _meshCollider.transform.localPosition;
    }

    public void CalculateContactdepth(Collider other)
    {
        if (other == null)
        {
            contactDepth = -_wheelSuspension.extensionDistance;
            transform.localPosition = new Vector3(0, contactDepth, 0);
            return;
        }

        bool significantOverlap;
        do
        {
            var ownBodyTransform = _meshCollider.transform;
            var collisionBodyTransform = other.transform;
            significantOverlap = Physics.ComputePenetration(_meshCollider, ownBodyTransform.position, ownBodyTransform.rotation,
                other, collisionBodyTransform.position, collisionBodyTransform.rotation, out Vector3 direction, out float distance);
            if (significantOverlap)
            {
                float penetration = Vector3.Dot(direction * distance, _meshCollider.transform.right);
                contactDepth = Math.Min(contactDepth+penetration, _wheelSuspension.compressionDistance);
                _meshCollider.transform.localPosition = new Vector3(0, contactDepth, 0);
                significantOverlap = penetration > 0.0001f && _wheelSuspension.compressionDistance > contactDepth;
            }
        } while (significantOverlap);
    }

    private void Update()
    {
        if (_lastFrameCollision < Time.frameCount)
        {
            //_meshCollider.transform.localPosition = _localPosition;
        }
    }

    public void CollisionEnter(Collision collision)
    {
        HandleCollision(collision);
        var localPosition = _meshCollider.transform.localPosition;
        Debug.Log(collision.contacts[0].thisCollider.gameObject.transform.parent.parent.name + " " + localPosition +
                  " " + collision.collider.gameObject.name + " " + Time.frameCount + " Enter");
    }


    public void CollisionStay(Collision collision)
    {
        Debug.Log(_meshCollider.transform.localPosition);
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        if (_lastFrameCollision < Time.frameCount)
        {
            //da gekippt anstatt up right 
            // var velocityYAxis = Math.Abs(Vector3.Dot(collision.relativeVelocity, _meshCollider.transform.right));
            // var movingDistance = velocityYAxis * Time.fixedDeltaTime;
            // Debug.Log("Collision:"+ movingDistance + " " + collision.relativeVelocity + " tansfrom"+ _meshCollider.transform.right+ "// " + Time.frameCount);
            //Change meshColliderPosition
            // _meshCollider.transform.localPosition += new Vector3(0.0f, movingDistance, 0.0f);
            var colA = collision.contacts[0].thisCollider;
            var colB = collision.contacts[0].otherCollider;

            var colBTransform = colB.transform;
            var colATransform = colA.transform;
            Physics.ComputePenetration(colA, colATransform.position, colATransform.rotation, colB,
                colBTransform.position, colBTransform.rotation, out _, out float dist);
            _meshCollider.transform.localPosition += new Vector3(0.0f, dist + 0.0001f, 0.0f);

            _lastFrameCollision = Time.frameCount;
        }
    }
}