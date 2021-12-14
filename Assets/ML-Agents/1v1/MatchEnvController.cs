using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MatchEnvController : MonoBehaviour
{
    private TeamController _teamController;
    
    private SimpleMultiAgentGroup _teamBlueAgentGroup;
    private SimpleMultiAgentGroup _teamOrangeAgentGroup;
    private Transform _ball;

    private MapData _mapData;

    private float _episodeLength;
    private float _lastResetTime;
    
    // Start is called before the first frame update
    void Start()
    {
        _teamController = transform.GetComponent<TeamController>();

        _teamBlueAgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in _teamController.TeamBlue)
        {
            _teamBlueAgentGroup.RegisterAgent(agent.GetComponent<OneVsOneAgent>());
        }
        _teamOrangeAgentGroup = new SimpleMultiAgentGroup();
        foreach (var agent in _teamController.TeamOrange)
        {
            _teamOrangeAgentGroup.RegisterAgent(agent.GetComponent<OneVsOneAgent>());
        }
        _ball = transform.Find("Ball");
        
        _mapData = transform.Find("World").Find("Rocket_Map").GetComponent<MapData>();
        
        _episodeLength = transform.GetComponent<MatchTimeController>().matchTimeSeconds;
        _lastResetTime = Time.time;
    }

    public void Reset()
    {
        // End episode for all agents
        _teamBlueAgentGroup.EndGroupEpisode();
        _teamOrangeAgentGroup.EndGroupEpisode();
        
        // Reset environment
        _teamController.SpawnTeams();
        
        _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        _mapData.ResetIsScored();

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
            _teamBlueAgentGroup.AddGroupReward(5.0f);
            _teamOrangeAgentGroup.AddGroupReward(-5.0f);
            
            Reset();
        }
        if (_mapData.isScoredOrange)
        {
            _teamBlueAgentGroup.AddGroupReward(-5.0f);
            _teamOrangeAgentGroup.AddGroupReward(5.0f);
            
            Reset();
        }
    }
}
