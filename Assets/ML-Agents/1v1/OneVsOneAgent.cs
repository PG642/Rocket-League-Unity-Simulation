using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class OneVsOneAgent : Agent
{
    // Start is called before the first frame update

    private Rigidbody _rb, _rbBall, _rbEnemy;

    private float _episodeLength;
    private float _lastResetTime;

    private Transform _ball, _enemy;

    private CubeJumping _jumpControl;
    private CubeController _controller;
    private CubeBoosting _boostControl;
    private CubeGroundControl _groundControl;
    private CubeAirControl _airControl;

    private MapData _mapData;

    public InputManager InputManager;


    void Start()
    {
        _episodeLength = transform.parent.GetComponent<MatchTimeController>().matchTimeSeconds;

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

        if (transform.name.Equals("1v1Agent0"))
            _enemy = transform.parent.Find("1v1Agent1");
        else
            _enemy = transform.parent.Find("1v1Agent0");
        _rbEnemy = _enemy.GetComponent<Rigidbody>();

        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();
    }

    public override void OnEpisodeBegin()
    {
        //Respawn Cars
        transform.parent.GetComponent<TeamController>().SpawnTeams();

        //Reset Ball
        _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
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

        //Enemy position
        var enemyXNormalized = (_enemy.localPosition.x + 60f) / 120f;
        var enemyYNormalized = _enemy.localPosition.y / 20f;
        var enemyZNormalized = (_enemy.localPosition.z + 41f) / 82f;
        sensor.AddObservation(new Vector3(enemyXNormalized, enemyYNormalized, enemyZNormalized));
        //Enemy rotation, already normalized
        sensor.AddObservation(_enemy.rotation);
        //Enemy velocity
        sensor.AddObservation(_rbEnemy.velocity / 23f);
        //Enemy angular velocity
        sensor.AddObservation(_rbEnemy.angularVelocity / 5.5f);

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
    }

    public void Update()
    {
        if (Time.time - _lastResetTime > _episodeLength)
        {
            Reset();
        }

        float ballDistanceReward = 0.01f * (1 - (Vector3.Distance(_ball.position, transform.position) / _mapData.diag));
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
