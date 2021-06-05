using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapData : MonoBehaviour
{
    public float diag;
    public bool isScoredBlue = false;
    public bool isScoredOrange = false;
    public int blueScore = 0;
    public int orangeScore = 0;

    private GoalController _blueGoal;
    private GoalController _redGoal;

    private GUIStyle _style;


    void Start()
    {
        diag = Vector3.Distance(transform.Find("diagP1").position, transform.Find("diagP2").position);
        _blueGoal = transform.Find("GoalLines").transform.Find("GoalLineBlue").GetComponent<GoalController>();
        _redGoal = transform.Find("GoalLines").transform.Find("GoalLineRed").GetComponent<GoalController>();

        _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        _style.fontSize = 25;
        _style.fontStyle = FontStyle.Bold;
        _style.alignment = TextAnchor.UpperCenter;
    }

    public void NotifyScore(bool isBlueTeam)
    {
        if (isBlueTeam)
        {
            isScoredBlue = true;
            blueScore++;
        }
        else
        {
            isScoredOrange = true;
            orangeScore++;
        }
    }

    public void ResetIsScored()
    {
        isScoredBlue = false;
        isScoredOrange = false;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 75, 0f, 150, 130), $"{blueScore:D2} : {orangeScore:D2}", _style);
    }
}
