using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class NormalMatchAgent: Agent
{
    // Start is called before the first frame update

    private Rigidbody _rb, _rbBall;

    private float _episodeLength = 10f;
    private float _lastResetTime;

    private Transform _ball;

    private CubeJumping _jumpControl;
    private CubeController _controller;
    private CubeBoosting _boostControl;
    private CubeGroundControl _groundControl;
    private CubeAirControl _airControl;

    private MapData _mapData;

    public InputManager InputManager;


    void Start()
    {
        InputManager = GetComponent<InputManager>();
        InputManager.isAgent = true;

        _rb = GetComponent<Rigidbody>();
        _airControl = GetComponentInChildren<CubeAirControl>();
        _jumpControl = GetComponentInChildren<CubeJumping>();
        _controller = GetComponentInChildren<CubeController>();
        _boostControl = GetComponentInChildren<CubeBoosting>();
        _groundControl = GetComponentInChildren<CubeGroundControl>();

        _ball = transform.parent.Find("Ball");
        _rbBall = _ball.GetComponent<Rigidbody>();

        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();
    }

    public override void OnEpisodeBegin()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(_rb.velocity);
        sensor.AddObservation(_rb.angularVelocity);
        //Ball
        sensor.AddObservation(_ball.localPosition);
        sensor.AddObservation(_rbBall.velocity);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // set inputs
        InputManager.throttleInput = actionBuffers.ContinuousActions[0];
        InputManager.steerInput = actionBuffers.ContinuousActions[1];
        InputManager.yawInput = actionBuffers.ContinuousActions[1];
        InputManager.pitchInput = actionBuffers.ContinuousActions[2];
        InputManager.rollInput = 0;
        if (actionBuffers.ContinuousActions[3] > 0) InputManager.rollInput = 1;
        if (actionBuffers.ContinuousActions[3] < 0) InputManager.rollInput = -1;

        InputManager.isBoost = actionBuffers.ContinuousActions[4] > 0;
        InputManager.isDrift = actionBuffers.ContinuousActions[5] > 0;
        InputManager.isAirRoll = actionBuffers.ContinuousActions[6] > 0;

        InputManager.isJump = actionBuffers.ContinuousActions[7] > 0;
        InputManager.isJumpUp = actionBuffers.ContinuousActions[8] > 0;
        InputManager.isJumpDown = actionBuffers.ContinuousActions[9] > 0;
        
        float ballDistanceReward = 0.01f * (1-(Vector3.Distance(_ball.position,transform.position)/_mapData.diag));
        AddReward(ballDistanceReward);
    }


    public void AddRewardToAgent(float increment)
    {
        AddReward(increment);
    }
    
    public void SetRewardOfAgent(float reward)
    {
        SetReward(reward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        InputManager.isAgent = false;
    }

    private void Reset()
    {
        _lastResetTime = Time.time;
        EndEpisode();
    }
}
