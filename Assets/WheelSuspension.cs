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
    
    private float _sprungMass { get; set; } //How much weight the wheel has to support
    private float _contactDepth { get; set; } //How much the suspension is compressed
    private float _contactSpeed { get; set; } //How fast the suspension moves
    private Vector3 _contactVelocity { get; set; } //Velocity Vector of colliding object
    private float _suspensionForce { get; set; } //How much force the suspension applies to the car

    private Rigidbody _carRb;

    public GameObject groundTrigger;
    public GameObject suspensionTrigger;
    public GameObject displacementTrigger;

    void Start()
    {
        _carRb = GetComponentInParent<Rigidbody>();
        WheelCollider[] colliders = GetComponentsInChildren<WheelCollider>();
        CubeWheel cb = GetComponentInChildren<CubeWheel>();

        foreach (WheelCollider collider in colliders)
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

        //Für testzwecke, nicht final
        groundTrigger.GetComponent<WheelCollider>().isTrigger = false;

    }
    
    void Update()
    {

    }
    
}
