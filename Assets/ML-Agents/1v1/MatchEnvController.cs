using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TestScenarios.JsonClasses;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MatchEnvController : MonoBehaviour
{
    private TeamController _teamController;
    
    private HashSet<OneVsOneAgent> _teamBlueAgentGroup;
    private HashSet<OneVsOneAgent> _teamOrangeAgentGroup;
    private Transform _ball;
    private Transform _spawnPositions;

    private MapData _mapData;

    public int maxSteps = 2000;
    private int _stepCount;

    // Start is called before the first frame update
    void Start()
    {
        _teamController = transform.GetComponent<TeamController>();
        _teamController.Initialize();

        _teamBlueAgentGroup = new HashSet<OneVsOneAgent>();
        foreach (var agentGameObject in _teamController.TeamBlue)
        {
            var agent = agentGameObject.GetComponent<OneVsOneAgent>();
            if (!_teamBlueAgentGroup.Contains(agent))
            {
                _teamBlueAgentGroup.Add(agent);
            }
        }
        _teamOrangeAgentGroup = new HashSet<OneVsOneAgent>();
        foreach (var agentGameObject in _teamController.TeamOrange)
        {
            var agent = agentGameObject.GetComponent<OneVsOneAgent>();
            if (!_teamOrangeAgentGroup.Contains(agent))
            {
                _teamOrangeAgentGroup.Add(agent);
            }
        }
        _ball = transform.Find("Ball");
        _ball.GetComponent<Ball>().stopSlowBall = false;

        _mapData = transform.Find("World").Find("Rocket_Map").GetComponent<MapData>();
        _spawnPositions = transform.Find("World").Find("Rocket_Map").Find("SpawnPositions");
    }

    public void Reset()
    {
        SwapTeams();
        
        ResetBall();

        // End episode for all agents
        int spawnPosition = Random.Range(0, _spawnPositions.Find("Blue").Find("Spawn").childCount);
        foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
        {
            Debug.Log("111" + agent.team + " " + spawnPosition);
            ResetAgent(agent, TeamController.Team.BLUE, spawnPosition);
            Debug.Log("222" + agent.team + " " + spawnPosition);
        }

        foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
        {
            Debug.Log("333" + agent.team + " " + spawnPosition);
            ResetAgent(agent, TeamController.Team.ORANGE, spawnPosition);
            Debug.Log("444" + agent.team + " " + spawnPosition);
        }
        
        // Reset environment
        // _teamController.SpawnTeams();

        _mapData.ResetIsScored();

        // rotate the whole environment
        // var rotation = Random.Range(1, 3);
        // var rotationAngle = rotation * 90.0f;
        // transform.Rotate(0.0f, rotationAngle, 0.0f);

        // Reset start time of episode to now
        //_lastResetTime = Time.time;
        _stepCount = 0;
    }

    void FixedUpdate()
    {
        ++_stepCount;
        if (_stepCount >= maxSteps && maxSteps > 0)
        {
            Reset();
        }
        
        if (_mapData.isScoredBlue)
        {
            foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
            {
                agent.AddReward(100.0f);
            }

            foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
            {
                agent.AddReward(-100.0f);
            }
            
            Reset();
        }
        if (_mapData.isScoredOrange)
        {
            foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
            {
                agent.AddReward(-100.0f);
            }

            foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
            {
                agent.AddReward(100.0f);
            }
            
            Reset();
        }
    }
    
    private void SwapTeams()
    {
        _teamController.SwapTeams();
        (_teamOrangeAgentGroup, _teamBlueAgentGroup) = (_teamBlueAgentGroup, _teamOrangeAgentGroup);
    }
    
    public void ResetBall()
    {
        Transform ballSpawn = _spawnPositions.Find("Ball").Find("Center");
        _ball.localPosition = new Vector3(0, _ball.GetComponentInChildren<SphereCollider>().radius, 0) + ballSpawn.localPosition;
        _ball.localRotation = ballSpawn.localRotation;
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void ResetAgent(OneVsOneAgent agent, TeamController.Team team, int spawnPosition)
    {
        Transform blueSpawns = _spawnPositions.Find("Blue").Find("Spawn");
        Transform orangeSpawns = _spawnPositions.Find("Orange").Find("Spawn");
        agent.EndEpisode();
        agent.transform.localPosition = new Vector3(0f,0.1701f,0f) + ((team == TeamController.Team.BLUE) ? 
            blueSpawns.GetChild(spawnPosition).localPosition :
            orangeSpawns.GetChild(spawnPosition).localPosition);
        Debug.Log(team + " " + spawnPosition);
        agent.transform.localRotation = ((team == TeamController.Team.BLUE) ?
            blueSpawns.GetChild(spawnPosition).localRotation :
            orangeSpawns.GetChild(spawnPosition).localRotation);

        agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        agent.GetComponentInChildren<CubeJumping>().Reset();
    }
}
