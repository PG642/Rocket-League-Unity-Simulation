using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using ML_Agents.Goalkeeper;
using UnityEngine.UI;

public class TopScorerAgent : PGBaseAgent
{
    [SerializeField] public GoalkeeperEnvironmentParameters defaultParameter;

    private GoalkeeperEvironmentHandler _handler;
    public int Difficulty = 0;

    private Rigidbody rbBall;
    private Ball ball;

    private Transform _ball, _shootAt;
    private Vector3 _midFieldPosition;
    private bool _ballTouched = false;
    private float _minDistance = 0.0f;
    [SerializeField] Text _seedText;

    protected override void Start()
    {
        base.Start();
        _handler = new GoalkeeperEvironmentHandler(transform.root.gameObject, defaultParameter);
        
        _ball = _handler.environment.GetComponentInChildren<Ball>().transform;
        rbBall = _ball.GetComponent<Rigidbody>();
        ball = _ball.GetComponent<Ball>();

        _midFieldPosition = Vector3.zero;
        _shootAt = transform.parent.Find("ShootAt");

        _handler.ResetParameter();
        
    }

    public override void OnEpisodeBegin()
    {
        _handler.ResetParameter();
        _seedText.text = "Seed: " + _handler.activeSeed;
        _ballTouched = false;
        // var diff = UnityEngine.Random.Range(0, 200) % 2;
        // switch (diff)
        // {
        //     case 0: OnEpisodeBeginDifficulty2(); break;
        //     case 1: OnEpisodeBeginDifficultyDefault(); break;
        //     default: throw new Exception("Difficulty does not exist");
        // }
        // OnEpisodeBeginDifficulty2();
        // OnEpisodeBegiEasy();
        OnEpisodeBeginDifficultyDefault();
        ball.ResetValues();
        mapData.ResetIsScored();
        SetReward(0f);
    }

    private void OnEpisodeBegiEasy()
    {
        Vector3 startPosition = _midFieldPosition + new Vector3(25f, 0.17f, UnityEngine.Random.Range(-15f, 15f));
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), _handler.boost_amount);
        Debug.Log("Agent Position");
        Debug.Log(startPosition.ToString("F6"));

        
        //Reset Ball
        float ball_z_pos = UnityEngine.Random.Range(0, 9) % 2 == 0 ? 6f : -6f;
        _ball.localPosition = new Vector3(45f, UnityEngine.Random.Range(2f, 5f), ball_z_pos + UnityEngine.Random.Range(-1f, 1f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        // Debug.Log("Ball Position");
        // Debug.Log(_ball.localPosition.ToString("F6"));

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(_ball.localPosition.x, 0f, 0f);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
    }

    private void OnEpisodeBeginDifficulty2()
    {
        Vector3 startPosition = _midFieldPosition + new Vector3(UnityEngine.Random.Range(20f, 25f), 0.17f, UnityEngine.Random.Range(-15f, 15f));
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), _handler.boost_amount);
        // Debug.Log("Agent Position");
        // Debug.Log(startPosition.ToString("F6"));
        
        //Reset Ball
        float ball_z_pos = UnityEngine.Random.Range(0, 9) % 2 == 0 ? 9f : -9f;
        _ball.localPosition = new Vector3(UnityEngine.Random.Range(44f, 46f), UnityEngine.Random.Range(2f, 10f), ball_z_pos + UnityEngine.Random.Range(-1f, 1f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(_ball.localPosition.x, 0f, 0f);

        // Debug.Log("Shoot At Position");
        // Debug.Log(_shootAt.localPosition.ToString("F6"));

        // Debug.Log("Ball Position");
        // Debug.Log(_ball.localPosition.ToString("F6"));

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
    }

    private void OnEpisodeBeginDifficultyDefault()
    {
        //Reset Car
        // Vector3 startPosition = _midFieldPosition + new Vector3(UnityEngine.Random.Range(20f, 25f), 0.17f, UnityEngine.Random.Range(-15f, 15f));
        Vector3 startPosition = _midFieldPosition + new Vector3(25f, 0.17f, UnityEngine.Random.Range(-15f, 15f));
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f + UnityEngine.Random.Range(-20f, 20f), 0f), _handler.boost_amount);

        // Debug.Log("Agent Position");
        // Debug.Log(startPosition.ToString("F6"));

        //Reset Ball
        _ball.localPosition = new Vector3(50.0f, UnityEngine.Random.Range(0.75f, 10f), UnityEngine.Random.Range(-20f, 20f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(0f, 7f), rb.position.z);

        // Debug.Log("Shoot At Position");
        // Debug.Log(_shootAt.localPosition.ToString("F6"));

        // Debug.Log("Ball Position");
        // Debug.Log(_ball.localPosition.ToString("F6"));

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
        _minDistance = Vector3.Distance(_ball.position, transform.position);
    }

    private void resetFromErrorState()
    {
        Debug.Log($"Error state occured in step {StepCount}; DeltaTime {Time.deltaTime}");
        SetReward(0f);
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (ball.BallStuck)
        {
            resetFromErrorState();
            return;
        }
        //Car position
        Vector3 car_position = NormalizePosition(transform);
        if (checkVec(car_position, "car_localPosition", -1f))
        {
            resetFromErrorState();
            return;
        }

        //Car rotation, already normalized
        Quaternion rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        checkQuaternion(rotation, "rotation", -1f);
        //Car velocity
        Vector3 velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z) / 23f;
        if (checkVec(velocity, "velocity", -1f))
        {
            resetFromErrorState();
            return;
        }

        //Car angular velocity
        Vector3 angular_velocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, rb.angularVelocity.z) / 7.3f;
        if (checkVec(angular_velocity, "angular_velocity", -1f))
        {
            resetFromErrorState();
            return;
        }

        //Ball position
        Vector3 ball_position = NormalizePosition(_ball);
        if (checkVec(ball_position, "ball_localPosition", -1f))
        {
            resetFromErrorState();
            return;
        }

        //Ball velocity
        Vector3 ball_velocity = new Vector3(rbBall.velocity.x, rbBall.velocity.y, rbBall.velocity.z) / 60f;
        if (checkVec(ball_velocity, "ball_velocity", -1f))
        {
            resetFromErrorState();
            return;
        }

        // Boost amount
        var boostAmount = boostControl.boostAmount / 100f;
        if (float.IsNaN(boostAmount) || float.IsInfinity(boostAmount))
        {
            Debug.Log("boost_amount is inf or nan");
            boostAmount = 0f;
        }
        if (boostAmount < 0f || boostAmount > 1f)
        {
            Debug.LogWarning($"boostAmount is {boostAmount}, was expected to be in interval [0, 1]");
        }

        sensor.AddObservation(car_position);
        sensor.AddObservation(rotation);
        sensor.AddObservation(velocity);
        sensor.AddObservation(angular_velocity);
        sensor.AddObservation(ball_position);
        sensor.AddObservation(ball_velocity);
        sensor.AddObservation(boostAmount);

        // Relative position car to ball
        float relativeXPos = ((_ball.localPosition.x + 60f) - (transform.localPosition.x + 60f)) / 120f;
        float relativeYPos = (_ball.localPosition.y - transform.localPosition.y) / 20f;
        float relativeZPos = ((_ball.localPosition.z + 41f) - (transform.localPosition.z + 41f)) / 82f;
        sensor.AddObservation(new Vector3(relativeXPos, relativeYPos, relativeZPos));
    }

    private void Reset()
    {
        // Debug.Log(GetCumulativeReward());
        EndEpisode();
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    protected override void AssignReward()
    {
        RewardDirection();
        if (mapData.isScoredBlue)
        {
            // Agent scored a goal
            AddReward(1f);
            Reset();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Ball"))
        {
            if (!_ballTouched)
            {
                // AddReward(0.1f);
                // Debug.Log(GetCumulativeReward());
                _ballTouched = true;
            }
        }
    }

    private void RewardDirection()
    {
        float speed = rb.velocity.magnitude / 23.0f;
        float lookAtTarget = Vector3.Dot((_ball.position - transform.position).normalized, rb.velocity.normalized);
        AddReward((lookAtTarget * speed / 650.0f) / 10.0f);
    }

    private void RewardCloseness()
    {
        float currentDistance = Vector3.Distance(_ball.position, transform.position);
        if (currentDistance < _minDistance)
        {
            _minDistance = currentDistance;
            if (!_ballTouched)
            {
                float agentBallDistanceReward = 0.0001f *  Mathf.Pow((1 - (Vector3.Distance(_ball.position, transform.position) / mapData.diag)), 0.5f);
                //Debug.Log(agentBallDistanceReward);
                AddReward(agentBallDistanceReward);
            }
        }
    }

    private void PenalizeDistance()
    {
        if (!_ballTouched)
        {
            AddReward(-0.01f / 800f * (1 - (Vector3.Distance(_ball.position, transform.position) / mapData.diag)));
        }
    }
}
