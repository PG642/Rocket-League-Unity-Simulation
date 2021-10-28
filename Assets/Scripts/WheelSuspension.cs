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

    [Tooltip("Max distance the spring can be compressed above its resting point")]
    [Range(0, float.MaxValue)]
    public float compressionDistance;
    
    

    public  Rigidbody _carRb;
    public GameObject suspensionCollider;
    public GameObject displacementCollider;
    public GameObject groundTrigger;
    public GameObject upperCircle;
    public GameObject lowerCircle;
    public GameObject rectangle;

    

    private WheelCollider _displacementCollider;

    void Start()
    {
        _carRb = GetComponentInParent<Rigidbody>();

        suspensionCollider.transform.localScale = new Vector3(2 * radius, 0.001f, 2 * radius);
        lowerCircle.transform.localScale = new Vector3(2 * radius, 0.0001f, 2 * radius);
        upperCircle.transform.localScale = new Vector3(2 * radius, 0.0001f, 2 * radius);
        rectangle.transform.localScale = new Vector3(compressionDistance + extensionDistance, 0.0001f, 2*radius);

        lowerCircle.transform.localPosition = new Vector3(0, -extensionDistance, 0);
        upperCircle.transform.localPosition = new Vector3(0, compressionDistance, 0);
        rectangle.transform.localPosition = new Vector3(0, (compressionDistance - extensionDistance)/2, 0);
        displacementCollider.transform.localPosition = new Vector3(0, compressionDistance, 0);

        _displacementCollider = displacementCollider.GetComponent<WheelCollider>();
        _displacementCollider.radius = radius;
         _displacementCollider.suspensionDistance = 0;
        WheelFrictionCurve f = _displacementCollider.forwardFriction;
        f.stiffness = 0;
        _displacementCollider.forwardFriction = f;
        f = _displacementCollider.sidewaysFriction;
        f.stiffness = 0;
        _displacementCollider.sidewaysFriction = f;

        CubeWheel cb = GetComponentInChildren<CubeWheel>();
        cb.isFrontWheel = isFrontWheel;

    }
    
}
