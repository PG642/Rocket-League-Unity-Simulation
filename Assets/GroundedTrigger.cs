using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedTrigger : MonoBehaviour
{
    private GroundTrigger _groundTrigger;

    // Start is called before the first frame update
    void Awake()
    {
        _groundTrigger = GetComponentInParent<GroundTrigger>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Boostpad") && !other.CompareTag("GoalLine"))
        {
            _groundTrigger.TriggerEnter(other);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Boostpad") && !other.CompareTag("GoalLine"))
        {
            _groundTrigger.TriggerStay(other);
        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Boostpad") && !other.CompareTag("GoalLine"))
        {
            _groundTrigger.TriggerExit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
