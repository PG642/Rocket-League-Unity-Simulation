using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedTrigger : MonoBehaviour
{
    private GroundTrigger _groundTrigger;

    // Start is called before the first frame update
    void Start()
    {
        _groundTrigger = GetComponentInParent<GroundTrigger>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Boostpad") && !other.CompareTag("GoalLine"))
        {
            _groundTrigger.TriggerEnter();
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
