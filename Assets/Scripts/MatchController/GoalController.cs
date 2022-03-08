using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public bool isGoalLineBlue;
    public Transform ball;
    private MapData _mapData;

    private void Start()
    {
        _mapData = GetComponentInParent<MapData>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        var ballPosition = ball.localPosition;
        var transformPosition = transform.localPosition;
        if ((isGoalLineBlue && ballPosition.x < transformPosition.x) ||
            (!isGoalLineBlue && ballPosition.x > transformPosition.x))
        {
            _mapData.NotifyScore(!isGoalLineBlue);
        }
    }
}
