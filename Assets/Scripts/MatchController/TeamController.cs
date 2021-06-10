using MatchController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamController : MonoBehaviour
{
    // define the two different teams by colorname
    public enum Team
    {
        BLUE,
        ORANGE,
        UNDEFINED
    };
    // Start is called before the first frame update
    public int TeamSize = 2;
    public GameObject[] TeamBlue;
    public GameObject[] TeamOrange;
    public void Initialize()
    {
        // Set a team for each car in the environment
        InitializeTeams();
        // Spawn each car
        SpawnTeams();
    }


    // searches for all cars in the environment and selects a team for them
    void InitializeTeams()
    {
        TeamBlue = new GameObject[TeamSize];
        TeamOrange = new GameObject[TeamSize];
        int playerCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.tag == "ControllableCar")
            {
                if (playerCount % 2 == 0)
                {
                    TeamBlue[playerCount / 2] = child.gameObject;
                }
                else
                {
                    TeamOrange[playerCount / 2] = child.gameObject;
                }
                playerCount++;
            }
        }
    }


    public void SpawnTeams()
    {
        // Spawn cars from orange team
        foreach (GameObject orangeCar in TeamOrange)
        {
            transform.GetComponent<SpawnController>().SpawnCar(orangeCar, Team.ORANGE, false);
        }
        // Spawn cars from blue team
        foreach (GameObject blueCar in TeamBlue)
        {
            transform.GetComponent<SpawnController>().SpawnCar(blueCar, Team.BLUE, false);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    // returns a cars Team value
    public Team GetTeamOfCar(GameObject car)
    {
        foreach (GameObject orangeCar in TeamOrange)
        {
            if (car.GetInstanceID().Equals(orangeCar.GetInstanceID()))
            {
                return Team.ORANGE;
            }
        }
        foreach (GameObject blueCar in TeamBlue)
        {
            if (car.GetInstanceID().Equals(blueCar.GetInstanceID()))
            {
                return Team.BLUE;
            }
        }
        // if the gameobject wasn't in the orange team nor in the blue team it's team value is undefined
        return Team.UNDEFINED;
    }

}
