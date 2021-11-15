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

    private Transform _ball, _shootAt;
    private Vector3 _startPosition;


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

        _startPosition = transform.localPosition;

        _shootAt = transform.parent.Find("ShootAt");
        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();

        _lastResetTime = Time.time;
    }

    public override void OnEpisodeBegin()
    {
        //Reset Car
        _controller.ResetCar(_startPosition, Quaternion.Euler(0f, 90f, 0f));

        //Reset Ball
        _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(-53f, Random.Range(3f, 3f), Random.Range(-7f, 7f));

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();

        _mapData.ResetIsScored();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car position
        var carXNormalized = (transform.localPosition.x + 60f) / 120f;
        var carYNormalized = transform.localPosition.y / 20f;
        var carZNormalized = (transform.localPosition.z + 41f) / 82f;
        sensor.AddObservation(new Vector3(carXNormalized, carYNormalized, carZNormalized));

        //Car rotation, already normalized
        sensor.AddObservation(transform.rotation);
        //Car velocity
        sensor.AddObservation(_rb.velocity / 23f);

        //Car angular velocity
        sensor.AddObservation(_rb.angularVelocity / 5.5f);

        //Ball position
        var ballXNormalized = (_ball.localPosition.x + 60f) / 120f;
        var ballYNormalized = _ball.localPosition.y / 20f;
        var ballZNormalized = (_ball.localPosition.z + 41f) / 82f;
        sensor.AddObservation(new Vector3(ballXNormalized, ballYNormalized, ballZNormalized));

        //Ball velocity
        sensor.AddObservation(_rbBall.velocity / 60f);

        // Boost amount
        sensor.AddObservation(_boostControl._boostAmount / 100f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (InputManager.isAgent)
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
        }
        AssignReward();
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

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    private void AssignReward()
    {
        if (Time.time - _lastResetTime > _episodeLength)
        {
            AddReward(0.5f);
            AddReward((_ball.localPosition.x / 53f) / 2f);
            Reset();
        }
        if (_mapData.isScoredBlue)
        {
            // Agent scored a goal
            SetReward(1f);
            Reset();
        }
        if (_mapData.isScoredOrange)
        {
            // Agent got scored on
            SetReward(-1f);
            Reset();
        }
    }
}
