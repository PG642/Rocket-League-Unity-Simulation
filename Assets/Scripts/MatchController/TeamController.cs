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
        // Set team colors for the walls
        DyeWalls();
        // Spawn each car
        SpawnTeams();
    }



    void DyeWalls()
    {
        Transform rocketMap = transform.Find("World").Find("Rocket_Map");
        Material[] materials = rocketMap.Find("StaticMeshActor_2").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.0f, 0.48f, 1.0f));
        
        materials = rocketMap.Find("StaticMeshActor_4").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.0f, 0.48f, 1.0f));

        materials = rocketMap.Find("StaticMeshActor_5").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.0f, 0.48f, 1.0f));

        materials = rocketMap.Find("StaticMeshActor_1").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.8f, 0.4f, 0.0f));
        
        materials = rocketMap.Find("StaticMeshActor_3").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.8f, 0.4f, 0.0f));

        materials = rocketMap.Find("StaticMeshActor_6").GetComponent<Renderer>().materials;
        FindAndDyeMaterial(materials, "GChecker1", new Color(0.8f, 0.4f, 0.0f));


    }

    void FindAndDyeMaterial(Material[] materials, string name, Color color)
    {
        foreach (Material mat in materials)
        {
            if (mat.name.Contains(name))
            {
                mat.color = color;
            }
        }
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
                    Material[] materials = child.Find("CubeController").Find("Body Mesh").Find("Octane").GetComponent<Renderer>().materials;
                    FindAndDyeMaterial(materials, "Red", new Color(0.0f, 0.48f, 1.0f));
                }
                else
                {
                    TeamOrange[playerCount / 2] = child.gameObject;
                    Material[] materials = child.Find("CubeController").Find("Body Mesh").Find("Octane").GetComponent<Renderer>().materials;
                    FindAndDyeMaterial(materials, "Red", new Color(0.8f, 0.4f, 0.0f));
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