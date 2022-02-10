using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{

    public float damper;

    public float frontStiffness;
    public float rearStiffness;

    public GameObject frontAxle;

    public GameObject backAxle;

    public bool updateEveryFrame = false;
    

    // Start is called before the first frame update
    void Start()
    {
        var suspensionCollidersFront = frontAxle.GetComponentsInChildren<SuspensionCollider>();
        var suspensionCollidersRear = backAxle.GetComponentsInChildren<SuspensionCollider>();
        foreach (var suspensionCollider in suspensionCollidersFront)
        {
            suspensionCollider.stiffness = frontStiffness;
            
        }
            
        foreach (var suspensionCollider in suspensionCollidersRear)
        {
            suspensionCollider.stiffness = rearStiffness;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (updateEveryFrame)
        {
            var suspensionCollidersFront = frontAxle.GetComponentsInChildren<SuspensionCollider>();
            var suspensionCollidersRear = backAxle.GetComponentsInChildren<SuspensionCollider>();
            foreach (var suspensionCollider in suspensionCollidersFront)
            {
                suspensionCollider.stiffness = frontStiffness;
                suspensionCollider.damper = damper;

            }
            
            foreach (var suspensionCollider in suspensionCollidersRear)
            {
                suspensionCollider.stiffness = rearStiffness;
                suspensionCollider.damper = damper;
            }
        }
    }
}
