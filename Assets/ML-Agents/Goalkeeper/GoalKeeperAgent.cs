using System.Collections;
using System;
using System.Collections.Generic;
using ML_Agents.Goalkeeper;
using ML_Agents.Handler;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public class GoalKeeperAgent : PGBaseAgent
{
    // Start is called before the first frame update
    [SerializeField] public GoalkeeperEnvironmentParameters defaultParameter;

    private GoalkeeperEvironmentHandler _handler;

    private float _episodeLengthSeconds = 10f;
    private float _episodeLengthFrames;
    private float _lastResetFrame;

    private Rigidbody _rbBall;
    private Transform _ball, _shootAt;
    private Vector3 _startPosition;

    public float difficulty;

    void Start()
    {
        base.Start();
        _handler = new GoalkeeperEvironmentHandler(transform.root.gameObject, defaultParameter);
        _episodeLengthFrames = _episodeLengthSeconds / Time.fixedDeltaTime;
        _ball = _handler.environment.GetComponentInChildren<Ball>().transform;
        _rbBall = _ball.GetComponent<Rigidbody>();

        _startPosition = transform.position;

        _shootAt = transform.parent.Find("ShootAt");

        _lastResetFrame = Time.frameCount;
        
        _handler.UpdateEnvironmentParameters();
        _handler.ResetParameter();

    }

    public override void OnEpisodeBegin()
    {
        //Reset Car
        controller.ResetCar(_startPosition, Quaternion.Euler(0f, 90f, 0f));

        ResetShoot();

        mapData.ResetIsScored();
    }

    public static Vector3 GetLocalPositionTarget(Difficulty difficulty)
    {
        // Tor ist von -8.8 bis 8.8 breit und von 0 bis 6.5 hoch
        switch (difficulty)
        {
            case Difficulty.EASY:
                return new Vector3(-53f, UnityEngine.Random.Range(0.9315f, 2.5f), UnityEngine.Random.Range(-2f, 2f));
            case Difficulty.MIDDLE:
                return new Vector3(-53f, UnityEngine.Random.Range(0.9315f, 4f), UnityEngine.Random.Range(-4f, 4f));
            case Difficulty.HARD:
                return new Vector3(-53f, UnityEngine.Random.Range(0.9315f, 5.6f),
                    UnityEngine.Random.Range(-7.9f, 7.9f));
            default:
                return new Vector3(-53f, UnityEngine.Random.Range(0.9315f, 2.5f), UnityEngine.Random.Range(-2f, 2f));
        }
    }

    public static Vector3 GetLocalPositionBall(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.EASY:
                return new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(0.9315f, 5f),
                    UnityEngine.Random.Range(-10f, 10f));
            case Difficulty.MIDDLE:
                return new Vector3(UnityEngine.Random.Range(-20f, 20f), UnityEngine.Random.Range(0.9315f, 10f),
                    UnityEngine.Random.Range(-20f, 20f));
            case Difficulty.HARD:
                return new Vector3(UnityEngine.Random.Range(-30f, 30f), UnityEngine.Random.Range(0.9315f, 19f),
                    UnityEngine.Random.Range(-39f, 39f));
            default:
                return new Vector3(UnityEngine.Random.Range(-5f, 0f), UnityEngine.Random.Range(0f, 10f),
                    UnityEngine.Random.Range(-10f, 10f));
        }
    }

    public float GetSpeed(Difficulty difficulty, Vector3 ballPosition, Vector3 targetPosition)
    {
        float scale;
        float dist = (ballPosition - targetPosition).magnitude;
        float minTime = 2f;
        float maxTime = Math.Max(minTime,Math.Min(_episodeLengthSeconds, dist / 5f));
        switch (difficulty)
        {
            case Difficulty.EASY:
                scale = UnityEngine.Random.Range(0.7f, 1f);
                break;
            case Difficulty.MIDDLE:
                scale = UnityEngine.Random.Range(0.4f, 0.7f);
                break;
            case Difficulty.HARD:
                scale = UnityEngine.Random.Range(0.1f, 4f);
                break;
            default:
                scale = UnityEngine.Random.Range(0.7f, 1f);
                break;
        }
        return dist * Math.Max(1 / minTime,scale / maxTime);
    }

    private void ResetShoot()
    {
        var localPositionBall = new Vector3();
        var localPositionTarget = new Vector3();
        var speed = 0.0f;


        switch ((int) difficulty)
        {
            case 0:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.EASY);
                localPositionBall = GetLocalPositionBall(Difficulty.EASY);
                speed = GetSpeed(Difficulty.EASY, localPositionBall, localPositionTarget);
                break;

            case 1:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.EASY);
                speed = GetSpeed(Difficulty.EASY, localPositionBall, localPositionTarget);
                break;

            case 2:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.EASY);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.EASY, localPositionBall, localPositionTarget);
                break;

            case 3:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.EASY);
                localPositionBall = GetLocalPositionBall(Difficulty.EASY);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 4:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.EASY, localPositionBall, localPositionTarget);
                break;

            case 5:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.EASY);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 6:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.EASY);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 7:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 8:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.HARD);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 9:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.HARD);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 10:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.HARD, localPositionBall, localPositionTarget);
                break;

            case 11:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.HARD);
                localPositionBall = GetLocalPositionBall(Difficulty.HARD);
                speed = GetSpeed(Difficulty.MIDDLE, localPositionBall, localPositionTarget);
                break;

            case 12:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.HARD);
                localPositionBall = GetLocalPositionBall(Difficulty.MIDDLE);
                speed = GetSpeed(Difficulty.HARD, localPositionBall, localPositionTarget);
                break;

            case 13:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.MIDDLE);
                localPositionBall = GetLocalPositionBall(Difficulty.HARD);
                speed = GetSpeed(Difficulty.HARD, localPositionBall, localPositionTarget);
                break;

            case 14:
                localPositionTarget =
                    GetLocalPositionTarget(Difficulty.HARD);
                localPositionBall = GetLocalPositionBall(Difficulty.HARD);
                speed = GetSpeed(Difficulty.HARD, localPositionBall, localPositionTarget);
                break;
        }

        //reset velocities
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        _ball.localPosition = localPositionBall;
        _shootAt.localPosition = localPositionTarget;
        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget(speed);

        mapData.ResetIsScored();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car position
        Vector3 carPosition = NormalizeVec(rb,VectorType.Position,EntityType.Car);
        checkNormalizedVec(carPosition, "carPosition", VectorType.Position);
        sensor.AddObservation(carPosition);

        //Car rotation, already normalized
        Quaternion carRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        checkQuaternion(carRotation, "carRotation", -1f);
        sensor.AddObservation(carRotation);

        //Car velocity
        Vector3 carVelocity = NormalizeVec(rb, VectorType.Velocity, EntityType.Car);
        checkNormalizedVec(carVelocity, "carVelocity", VectorType.Velocity);
        sensor.AddObservation(carVelocity);

        //Car angular velocity
        Vector3 carAngularVelocity = NormalizeVec(rb, VectorType.AngularVelocity, EntityType.Car);
        checkNormalizedVec(carAngularVelocity, "carAngularVelocity", VectorType.AngularVelocity);
        sensor.AddObservation(carAngularVelocity);

        //Ball position
        Vector3 ballPosition = NormalizeVec(_rbBall, VectorType.Position, EntityType.Ball);
        checkNormalizedVec(ballPosition, "ballPosition", VectorType.Position);
        sensor.AddObservation(ballPosition);

        //Ball velocity
        Vector3 ballVelocity = NormalizeVec(_rbBall, VectorType.Velocity, EntityType.Ball);
        checkNormalizedVec(ballVelocity, "ballVelocity", VectorType.Velocity);
        sensor.AddObservation(ballVelocity);

        // Boost amount
        float boostAmount = boostControl.boostAmount / 100f;
        if (float.IsNaN(boostAmount))
        {
            Debug.Log("Car: boostAmount == NaN");
            boostAmount = -1f;
        }

        sensor.AddObservation(boostAmount);
    }


    private void Reset()
    {
        _lastResetFrame = Time.frameCount;
        EndEpisode();
        _handler.ResetParameter();
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    protected override void AssignReward()
    {
        if (_rbBall.velocity.x > 0 || Time.frameCount - _lastResetFrame > _episodeLengthFrames)
        {
            SetReward(1f);

            Reset();
        }

        if (mapData.isScoredOrange)
        {
            // Agent got scored on
            SetReward(-1f);
            Reset();
        }
    }

    public enum Difficulty
    {
        EASY,
        MIDDLE,
        HARD
    }
}