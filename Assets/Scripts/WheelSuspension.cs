using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSuspension : MonoBehaviour
{
    [Tooltip("The Radius of the Wheel")]
    [Range(0, float.MaxValue)]
    public float radius;

    [Tooltip("Is this Wheel a front wheel?")]
    public bool isFrontWheel;

    [Tooltip("The stiffness of the spring")]
    [Range(0,float.MaxValue)]
    public float stiffness;

    [Tooltip("The dampening factor of the spring")]
    [Range(0, float.MaxValue)]
    public float damper;

    [Tooltip("Max distance the spring can extend under its resting point")]
    [Range(0, float.MaxValue)]
    public float extensionDistance;

    [Tooltip("Max distance the spring can be compressed above its resting pointt")]
    [Range(0, float.MaxValue)]
    public float compressionDistance;
    
    

    private Rigidbody _carRb;

    public GameObject groundTrigger;
    public GameObject suspensionTrigger;
    public GameObject displacementTrigger;

    public float sprungMass; //How much weight the wheel has to support
    public float contactDepth; //How much the suspension is compressed/extended
    public float contactSpeed; //How fast the suspension moves
    public Vector3 contactVelocity; //Velocity Vector of colliding object
    public float suspensionForce; //How much force the suspension applies to the car

    private WheelCollider[] _colliders;

    void Start()
    {
        _carRb = GetComponentInParent<Rigidbody>();
        _colliders = GetComponentsInChildren<WheelCollider>();
        CubeWheel cb = GetComponentInChildren<CubeWheel>();

        foreach (WheelCollider collider in _colliders)
        {
            collider.isTrigger = true;
            collider.radius = radius;
            collider.suspensionDistance = 0;
            WheelFrictionCurve f = collider.forwardFriction;
            f.stiffness = 0;
            collider.forwardFriction = f;

            f = collider.sidewaysFriction;
            f.stiffness = 0;
            collider.sidewaysFriction = f;
        }
        cb.isFrontWheel = isFrontWheel;
        groundTrigger.transform.localPosition = new Vector3(0, -extensionDistance, 0);
        displacementTrigger.transform.localPosition = new Vector3(0, compressionDistance, 0);
        float scale = (radius + extensionDistance + compressionDistance) * 2 * 121f/120f;
        Vector3 scaleVector = new Vector3(scale, 0, scale);
        groundTrigger.transform.localScale = scaleVector;

        //Für testzwecke, nicht final
        displacementTrigger.GetComponent<WheelCollider>().isTrigger = false;

    }
    
    void LateUpdate()
    {
        foreach (WheelCollider collider in _colliders)
        {
            collider.isTrigger = true;
            collider.radius = radius;
            collider.suspensionDistance = 0;
            WheelFrictionCurve f = collider.forwardFriction;
            f.stiffness = 0;
            collider.forwardFriction = f;

            f = collider.sidewaysFriction;
            f.stiffness = 0;
            collider.sidewaysFriction = f;
        }
        displacementTrigger.GetComponent<WheelCollider>().isTrigger = false;
        Debug.Log(groundTrigger.GetComponent<WheelCollider>().isTrigger);
    }
    
}
