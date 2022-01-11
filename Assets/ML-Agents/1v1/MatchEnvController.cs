using System.Collections;
using System.Collections.Generic;
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

    private MapData _mapData;

    private float _episodeLength;
    private float _lastResetTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _teamController = transform.GetComponent<TeamController>();

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
        
        _episodeLength = transform.GetComponent<MatchTimeController>().matchTimeSeconds;
        _lastResetTime = Time.time;
    }

    public void Reset()
    {
        // End episode for all agents
        foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
        {
            agent.EndEpisode();
            agent.transform.localPosition = new Vector3(Random.Range(-45f, -15f), 0.0f, Random.Range(-25.0f, 25.0f));
            var rotationToBall =
                Quaternion.LookRotation(_ball.position - agent.transform.position, Vector3.up);
            agent.transform.rotation = rotationToBall;
            
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            
            agent.GetComponentInChildren<CubeJumping>().Reset();
        }

        foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
        {
            agent.EndEpisode();
            agent.transform.localPosition = new Vector3(Random.Range(15f, 45f), 0.0f, Random.Range(-25.0f, 25.0f));
            var rotationToBall =
                Quaternion.LookRotation(_ball.localPosition - agent.transform.localPosition, Vector3.up);
            agent.transform.rotation = rotationToBall;
            
            agent.GetComponent<Rigidbody>().velocity = Vector3.zero;
            agent.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            agent.GetComponentInChildren<CubeJumping>().Reset();
        }
        
        // Reset environment
        // _teamController.SpawnTeams();

        _ball.localPosition = new Vector3(Random.Range(-25f, 25f), Random.Range(1f, 15f), Random.Range(-20f, 20f));
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        _mapData.ResetIsScored();

        // rotate the whole environment
        var rotation = Random.Range(1, 3);
        var rotationAngle = rotation * 90.0f;
        transform.Rotate(0.0f, rotationAngle, 0.0f);

        // Reset start time of episode to now
        _lastResetTime = Time.time;
    }
    
    void FixedUpdate()
    {
        if (Time.time - _lastResetTime > _episodeLength){
            Reset();
        }
        
        if (_mapData.isScoredBlue)
        {
            foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
            {
                agent.AddReward(5.0f);
            }

            foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
            {
                agent.AddReward(-5.0f);
            }
            
            Reset();
        }
        if (_mapData.isScoredOrange)
        {
            foreach (OneVsOneAgent agent in _teamBlueAgentGroup)
            {
                agent.AddReward(-5.0f);
            }

            foreach (OneVsOneAgent agent in _teamOrangeAgentGroup)
            {
                agent.AddReward(5.0f);
            }
            
            Reset();
        }
    }
}
