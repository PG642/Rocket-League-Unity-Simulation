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
    
    private List<ShotPredictionAgent> _teamBlueAgentGroup;
    private List<ShotPredictionAgent> _teamOrangeAgentGroup;
    private Transform _ball;
    private Transform _spawnPositions;

    private MapData _mapData;

    public int maxSteps = 120*20;
    private int _stepCount;

    // Start is called before the first frame update
    void Start()
    {
        _teamController = transform.GetComponent<TeamController>();
        _teamController.Initialize();
        maxSteps = 120 * 20;

        _teamBlueAgentGroup = new List<ShotPredictionAgent>();
        foreach (var agentGameObject in _teamController.TeamBlue)
        {
            var agent = agentGameObject.GetComponent<ShotPredictionAgent>();
            if (!_teamBlueAgentGroup.Contains(agent))
            {
                _teamBlueAgentGroup.Add(agent);
                agent.team = TeamController.Team.BLUE;
                agent.Init();
            }
        }
        _teamOrangeAgentGroup = new List<ShotPredictionAgent>();
        foreach (var agentGameObject in _teamController.TeamOrange)
        {
            var agent = agentGameObject.GetComponent<ShotPredictionAgent>();
            if (!_teamOrangeAgentGroup.Contains(agent))
            {
                _teamOrangeAgentGroup.Add(agent);
                agent.team = TeamController.Team.ORANGE;
                agent.Init();
            }
        }
        _teamBlueAgentGroup[0].enemy = _teamController.TeamOrange[0].transform;
        _teamBlueAgentGroup[0].rbEnemy = _teamBlueAgentGroup[0].enemy.GetComponent<Rigidbody>();
        _teamBlueAgentGroup[0].InitializeEnemyGoal();

        _teamOrangeAgentGroup[0].enemy = _teamController.TeamBlue[0].transform;
        _teamOrangeAgentGroup[0].rbEnemy = _teamOrangeAgentGroup[0].enemy.GetComponent<Rigidbody>();
        _teamOrangeAgentGroup[0].InitializeEnemyGoal();

        _ball = transform.Find("Ball");

        _mapData = transform.Find("World").Find("Rocket_Map").GetComponent<MapData>();
        _spawnPositions = transform.Find("World").Find("Rocket_Map").Find("SpawnPositions");

        ResetBall();
    }

    public void Reset()
    {
        // End episode for all agents
        foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
        {
            ResetAgent(agent, TeamController.Team.BLUE);
        }

        foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
        {
            ResetAgent(agent, TeamController.Team.ORANGE);
        }

        // Reset environment
        SwapTeams();
        ResetBall();
        _teamController.SpawnTeams();

        _mapData.ResetIsScored();
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
        foreach(ShotPredictionAgent agent in _teamOrangeAgentGroup)
        {
            agent.team = TeamController.Team.ORANGE;
            agent.InitializeEnemyGoal();
        }
        foreach (ShotPredictionAgent agent in _teamBlueAgentGroup)
        {
            agent.team = TeamController.Team.BLUE;
            agent.InitializeEnemyGoal();
        }
    }
    
    public void ResetBall()
    {
        Transform ballSpawn = _spawnPositions.Find("Ball").Find("Center");
        _ball.localPosition = new Vector3(0, _ball.GetComponentInChildren<SphereCollider>().radius, 0) + ballSpawn.localPosition;
        _ball.localRotation = ballSpawn.localRotation;
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _ball.GetComponent<Ball>().ResetValues();
    }

    private void ResetAgent(OneVsOneAgent agent, TeamController.Team team)
    {
        agent.EndEpisode();

        agent.GetComponentInChildren<CubeJumping>().Reset();
    }
}
