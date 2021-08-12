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

    public float frontBackSpring;

    public float sprungMass;

    private WheelCollider[] _wheels;

    private WheelCollider[] _wheelsFront;

    private WheelCollider[] _wheelsBack;

    public GameObject frontAxle;

    public GameObject backAxle;

    public bool updateEveryFrame = false;

    public bool useSprungMass = true;

    // Start is called before the first frame update
    void Start()
    {
        _wheels = GetComponentsInChildren<WheelCollider>();
        _wheelsFront = frontAxle.GetComponentsInChildren<WheelCollider>();
        _wheelsBack = backAxle.GetComponentsInChildren<WheelCollider>();
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
            if (useSprungMass)
            {
                wheel.sprungMass = sprungMass;
            }
        }

        foreach (WheelCollider wheel in _wheelsFront)
        {
            JointSpring suspensionSpring = new JointSpring();
            suspensionSpring.spring = spring * frontBackSpring;
            suspensionSpring.damper = damper;
            suspensionSpring.targetPosition = targetPosition;
            wheel.suspensionSpring = suspensionSpring;
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
                if (useSprungMass)
                {
                    wheel.sprungMass = sprungMass;
                }
            }

            foreach (WheelCollider wheel in _wheelsFront)
            {
                JointSpring suspensionSpring = new JointSpring();
                suspensionSpring.spring = spring * frontBackSpring;
                suspensionSpring.damper = damper;
                suspensionSpring.targetPosition = targetPosition;
                wheel.suspensionSpring = suspensionSpring;
            }
        }
    }
}
