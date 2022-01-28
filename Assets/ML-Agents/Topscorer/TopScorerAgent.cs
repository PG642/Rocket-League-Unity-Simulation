using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public class TopScorerAgent : PGBaseAgent
{
    // Start is called before the first frame update
    public int Difficulty = 0;

    private Rigidbody rbBall;
    private Ball ball;

    private static float _episodeLength = 5f;
    private static float _maxStepsPerEpisode = 120f * _episodeLength;
    private float _lastResetTime;

    private Transform _ball, _shootAt;
    private Vector3 _midFieldPosition;

    protected override void Start()
    {
        base.Start();
        _ball = transform.parent.Find("Ball");
        rbBall = _ball.GetComponent<Rigidbody>();
        ball = _ball.GetComponent<Ball>();

        _midFieldPosition = Vector3.zero;
        _shootAt = transform.parent.Find("ShootAt");

        _lastResetTime = Time.time;
    }

    public override void OnEpisodeBegin()
    {
        switch (Difficulty)
        {
            case 0: OnEpisodeBeginDifficulty0(); break;
            case 1: OnEpisodeBeginDifficulty1(); break;
            case 2: OnEpisodeBeginDifficulty2(); break;
            default: throw new Exception("Difficulty does not exist");
        }
        ball.ResetValues();
        mapData.ResetIsScored();
        SetReward(0f);
    }

    private void OnEpisodeBeginDifficulty0()
    {
        Vector3 startPosition = _midFieldPosition + new Vector3(30f, 0.17f, 0f);
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), 100f);

        _ball.localPosition = new Vector3(45f, 0.9315f, UnityEngine.Random.Range(-5f, 5f));
        rbBall.velocity = Vector3.zero;
        rbBall.angularVelocity = Vector3.zero;
        //_shootAt.localPosition = _ball.localPosition;
        //_ball.GetComponent<ShootBall>().ShootTarget();
    }

    private void OnEpisodeBeginDifficulty1()
    {
        // Reset Car
        Vector3 startPosition = _midFieldPosition + new Vector3(25f, 0.17f, UnityEngine.Random.Range(-5f, 5f));

        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), 100f);

        //Reset Ball
        _ball.localPosition = new Vector3(45f, 0.93f, UnityEngine.Random.Range(1, 10) % 2 == 0 ? -7f : 7f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(_ball.localPosition.x, 0f, 0f);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
    }

    private void OnEpisodeBeginDifficulty2()
    {
        Vector3 startPosition = _midFieldPosition + new Vector3(25f, 0.17f, UnityEngine.Random.Range(-15f, 15f));
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), 100f);


        //Reset Ball
        float ball_z_pos = UnityEngine.Random.Range(0, 9) % 2 == 0 ? 6f : -6f;
        _ball.localPosition = new Vector3(45f, UnityEngine.Random.Range(2f, 5f), ball_z_pos + UnityEngine.Random.Range(-1f, 1f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(_ball.localPosition.x, 0f, 0f);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
    }

    private void OnEpisodeBeginDifficultyDefault()
    {
        //Reset Car
        Vector3 startPosition = _midFieldPosition + new Vector3(0f, 0.17f, UnityEngine.Random.Range(-15f, 15f));

        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f + UnityEngine.Random.Range(-20f, 20f), 0f), 100f);

        //Reset Ball
        _ball.localPosition = new Vector3(UnityEngine.Random.Range(30f, 50f), UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(-20f, 20f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(0f, 7f), rb.position.z);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (ball.BallStuck)
        {
            SetReward(0f);
            Reset();
        }
        //Car position
        if (AddPositionNormalized(sensor, transform))
        {
            SetReward(0f);
            Reset();
        }

        //Car rotation, already normalized
        Quaternion rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        checkQuaternion(rotation, "rotation", -1f);
        sensor.AddObservation(rotation);
        //Car velocity
        Vector3 velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z) / 23f;
        if (checkVec(velocity, "velocity", -1f))
        {
            SetReward(0f);
            Reset();
        }
        sensor.AddObservation(velocity);

        //Car angular velocity
        Vector3 angular_velocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, rb.angularVelocity.z) / 7.3f;
        if (checkVec(angular_velocity, "angular_velocity", -1f))
        {
            SetReward(0f);
            Reset();
        }
        sensor.AddObservation(angular_velocity);

        //Ball position
        if (AddPositionNormalized(sensor, _ball))
        {
            SetReward(0f);
            Reset();
        }

        //Ball velocity
        Vector3 ball_velocity = new Vector3(rbBall.velocity.x, rbBall.velocity.y, rbBall.velocity.z) / 60f;
        if (checkVec(ball_velocity, "ball_velocity", -1f))
        {
            SetReward(0f);
            Reset();
        }
        sensor.AddObservation(ball_velocity);

        // Boost amount
        var boostAmount = boostControl._boostAmount / 100f;
        if (float.IsNaN(boostAmount) || float.IsInfinity(boostAmount))
        {
            Debug.Log("boost_amount is inf or nan");
            boostAmount = 0f;
        }
        if (boostAmount < 0f || boostAmount > 1f)
        {
            Debug.LogWarning($"boostAmount is {boostAmount}, was expected to be in interval [0, 1]");
        }
        sensor.AddObservation(boostAmount);
    }

    private void Reset()
    {
        _lastResetTime = Time.time;
        EndEpisode();
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    protected override void AssignReward()
    {
        if (StepCount > _maxStepsPerEpisode)// || rb.position.x > rbBall.position.x + 5.0f)
        {
            // Agent didn't score a goal
            AddReward(-1f);
            Reset();
        }
        else
        {
            float agentBallDistanceReward = 0.001f * (1 - (Vector3.Distance(_ball.position, transform.position) / mapData.diag));
            AddReward(agentBallDistanceReward);

            if (mapData.isScoredBlue)
            {
                // Agent scored a goal
                //AddShortEpisodeReward(0.2f);
                AddReward(1f);
                Reset();
            }
        }
    }

    private void AddShortEpisodeReward(float factor)
    {
        // adds a reward in range [0, factor]
        if (StepCount > _maxStepsPerEpisode)
        {
            return;
        }
        AddReward((1f - (StepCount / _maxStepsPerEpisode)) * factor);
    }

    private void AddFastShotReward(float factor)
    {
        // adds a reward in range [0, factor]
        if (Time.time - _lastResetTime > _episodeLength)
        {
            return;
        }
        AddReward((rbBall.velocity.magnitude / _ball.GetComponent<Ball>().maxVelocity) * factor);
    }
}
