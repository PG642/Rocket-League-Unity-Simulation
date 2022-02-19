using System;
using ML_Agents.Handler;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using PGAgent.ResetParameters;
using ML_Agents.Goalkeeper;


public class TopScorerAgent : PGBaseAgent
{
    public TopScorerAgentParameter defaultParameter;
    public float Difficulty;
    private PGEnvironmentHandler<TopScorerAgentParameter> _handler;
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
        _handler = new PGEnvironmentHandler<TopScorerAgentParameter>(transform.parent.gameObject, defaultParameter, new TopScorerAgentHandler());
        _ball = transform.parent.Find("Ball");
        rbBall = _ball.GetComponent<Rigidbody>();
        ball = _ball.GetComponent<Ball>();

        _midFieldPosition = Vector3.zero;
        _shootAt = transform.parent.Find("ShootAt");

        _lastResetTime = Time.time;
    }

    public override void OnEpisodeBegin()
    {
        _handler.ResetParameter();
        // var rand = UnityEngine.Random.Range(0, 200);
        // var diff = rand % 4;
        switch (Difficulty)
        {
            case 0f: OnEpisodeBeginDifficulty0(); break;
            case 1f: OnEpisodeBeginDifficulty1(); break;
            case 2f: OnEpisodeBeginDifficulty2(); break;
            case 3f: OnEpisodeBeginDifficulty3(); break;
            case 4f: OnEpisodeBeginDifficulty4(); break;
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

    private void OnEpisodeBeginDifficulty3()
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

    private void OnEpisodeBeginDifficulty4()
    {
        var ballLeft = UnityEngine.Random.Range(0, 100) % 2 == 0;
        Vector3 startPosition = _midFieldPosition + new Vector3(23f, 0f, ballLeft ? 10f : -10f);
        controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), 100f);

        _ball.localPosition = new Vector3(44f, 1.41579f, ballLeft ? -33f : 33f);
        rbBall.velocity = Vector3.zero;
        rbBall.angularVelocity = Vector3.zero;

        _shootAt.localPosition = new Vector3(44f, 10f, ballLeft ? -15f : 15f);
        _ball.GetComponent<ShootBall>().speed = new Vector2(20f, 20f);
        _ball.GetComponent<ShootBall>().ShootTarget();
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
    }

    private void Reset()
    {
        Debug.Log(GetCumulativeReward());
        _lastResetTime = Time.time;
        EndEpisode();
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    protected override void AssignReward()
    {
        AddReward(-(1 / _maxStepsPerEpisode));
        if (StepCount > _maxStepsPerEpisode)// || rb.position.x > rbBall.position.x + 5.0f)
        {
            // Agent didn't score a goal
            // AddReward(-1f);
            Reset();
        }
        else
        {
            // AddShortEpisodeReward(-0.2f);
            float agentBallDistanceReward = 0.00025f * (1 - (Vector3.Distance(_ball.position, transform.position) / mapData.diag));
            AddReward(agentBallDistanceReward);

            if (mapData.isScoredBlue)
            {
                // Agent scored a goal
                SetReward(1f);
                Reset();
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Ball"))
        {
            AddReward(0.1f);
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

    [Serializable]
    public class TopScorerAgentParameter : PGResetParameters
    {
        public float difficulty;

        public TopScorerAgentParameter() : base()
        {
            difficulty = 0f;
        }
    }

    public class TopScorerAgentHandler : ResetParameterHandler<TopScorerAgentParameter>
    {
        public void Handle(TopScorerAgentParameter parameters, GameObject environment)
        {
            environment.GetComponentInChildren<TopScorerAgent>().Difficulty = parameters.difficulty;
        }
    }
}
