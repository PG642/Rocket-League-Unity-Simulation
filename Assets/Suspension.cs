using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    public float mass;

    public float wheelDampingRate;

    public float suspensionDistance;

    public float spring;

    public float damper;

    public float targetPosition;

    public float sprungMass;

    private WheelCollider[] _wheels;

    public bool updateEveryFrame = false;

    public bool useSprungMass = true;

    // Start is called before the first frame update
    void Start()
    {
         _wheels = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in _wheels)
        {
            wheel.mass = mass;
            wheel.wheelDampingRate = wheelDampingRate;
            wheel.suspensionDistance = suspensionDistance;
            JointSpring suspensionSpring = new JointSpring();
            suspensionSpring.spring = spring;
            suspensionSpring.damper = damper;
            suspensionSpring.targetPosition = targetPosition;
            wheel.suspensionSpring = suspensionSpring;
            wheel.sprungMass = sprungMass;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (updateEveryFrame)
        {
            foreach (WheelCollider wheel in _wheels)
            {
                wheel.mass = mass;
                wheel.wheelDampingRate = wheelDampingRate;
                wheel.suspensionDistance = suspensionDistance;
                JointSpring suspensionSpring = new JointSpring();
                suspensionSpring.spring = spring;
                suspensionSpring.damper = damper;
                suspensionSpring.targetPosition = targetPosition;
                wheel.suspensionSpring = suspensionSpring;
            }
        }
    }
}
