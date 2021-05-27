using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    public float diag;
    public bool isScoredBlue;
    public bool isScoredRed;

    private GoalController _blueGoal;
    private GoalController _redGoal;


    void Start()
    {
        diag = Vector3.Distance(transform.Find("diagP1").position, transform.Find("diagP2").position);
        _blueGoal = transform.Find("GoalLines").transform.Find("GoalLineBlue").GetComponent<GoalController>();
        _redGoal = transform.Find("GoalLines").transform.Find("GoalLineRed").GetComponent<GoalController>();
    }

    private void Update()
    {
        isScoredBlue = _blueGoal.isScored;
        isScoredRed = _redGoal.isScored;
    }   

    public void Reset()
    {
        isScoredBlue = false;
        isScoredRed = false;
        _blueGoal.isScored = false;
        _redGoal.isScored = false;
    }
}
