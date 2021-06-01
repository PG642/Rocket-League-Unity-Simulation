using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GoalKeeperAgent : Agent
{
    // Start is called before the first frame update

    private Rigidbody _rb, _rbBall;

    private float _episodeLength = 10f;
    private float _lastResetTime;

    private Transform _ball;
    private Transform _target;

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

        _target = transform.parent.Find("World").Find("Rocket_Map").Find("Target");
        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();

        _lastResetTime = Time.time;
    }

    public override void OnEpisodeBegin()
    {
        //Reset Car
        transform.localPosition = new Vector3(-53f, 0f, 0);
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        //Reset Ball
        _ball.localPosition = new Vector3(0f, 1f, Random.Range(-8f, 8f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _target.localPosition = new Vector3(-53f, 1f, Random.Range(-8f, 8f));

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();

        _mapData.Reset();
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


        if (Time.time - _lastResetTime > _episodeLength)
        {
            AddReward(0.5f);
            Reset();
        }
        if(_mapData.isScoredBlue)
        {
            AddReward(-1f);
            Reset();
        }
        if (_mapData.isScoredRed)
        {
            AddReward(1f);
            Reset();
        }
        float ballDistanceReward = 0.01f * (1-(Vector3.Distance(_ball.position,transform.position)/_mapData.diag));
        AddReward(ballDistanceReward);

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
