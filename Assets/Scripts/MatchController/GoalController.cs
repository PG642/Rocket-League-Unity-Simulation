using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{


    public bool isGoalLineBlue;
    public bool isScored = false;
    public Transform ball;


    private void Start()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            if(isGoalLineBlue)
            {
                if(ball.position.x < transform.position.x)
                {
                    // Debug.Log("Ball passed blue GoalLine.");
                    isScored = true;
                }
            }
            else
            {
                if (ball.position.x > transform.position.x)
                {
                    // Debug.Log("Ball passed red GoalLine.");
                    isScored = true;
                }
            }
        }
    }


}
