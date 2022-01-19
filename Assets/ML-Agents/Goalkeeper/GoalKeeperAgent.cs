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


public class GoalKeeperAgent : Agent
{
    // Start is called before the first frame update
    [SerializeField]
    public GoalkeeperEnvironmentParameters defaultParameter;

    private GoalkeeperEvironmentHandler _handler;
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

    /// <summary>
    /// Contains the possible values for the discretized actions.
    /// </summary>
    private readonly float[] DISCRETE_ACTIONS = { -1f, -0.5f, 0f, 0.5f, 1f };

    /// <summary>
    /// Shows whether the action space of the agent is continuous, multi-discrete or mixed.
    /// </summary>
    private ActionSpaceType _actionSpaceType;
    
    void Start()
    {
        _handler = new GoalkeeperEvironmentHandler(defaultParameter);
        InputManager = GetComponent<InputManager>();
        InputManager.isAgent = true;

        ActionSpec actionSpec = GetComponent<BehaviorParameters>().BrainParameters.ActionSpec;
        _actionSpaceType = DetermineActionSpaceType(actionSpec);

        _rb = GetComponent<Rigidbody>();
        _airControl = GetComponentInChildren<CubeAirControl>();
        _jumpControl = GetComponentInChildren<CubeJumping>();
        _controller = GetComponentInChildren<CubeController>();
        _boostControl = GetComponentInChildren<CubeBoosting>();
        _groundControl = GetComponentInChildren<CubeGroundControl>();

        _ball = transform.parent.Find("Ball");
        _rbBall = _ball.GetComponent<Rigidbody>();

        _startPosition = transform.position;

        _shootAt = transform.parent.Find("ShootAt");
        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();

        _lastResetTime = Time.time;
        
        _handler.UpdateEnvironmentParameters();
        
        
    }

    public override void OnEpisodeBegin()
    {
        //Reset Car
        _controller.ResetCar(_startPosition, Quaternion.Euler(0f, 90f, 0f));

        //Reset Ball
        _ball.localPosition = new Vector3(UnityEngine.Random.Range(-10f, 0f), UnityEngine.Random.Range(0f, 20f), UnityEngine.Random.Range(-30f, 30f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(-53f, UnityEngine.Random.Range(3f, 3f), UnityEngine.Random.Range(-7f, 7f));

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();

        _mapData.ResetIsScored();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car position
        // TODO: Welche Normalisierung?? (alle /120f?)
        var carXNormalized = (transform.localPosition.x + 60f) / 120f;
        var carYNormalized = transform.localPosition.y / 20f;
        var carZNormalized = (transform.localPosition.z + 41f) / 82f;
        if (float.IsNaN(carXNormalized))
        {
            Debug.Log("Car: carXNormalized == NaN");
            carXNormalized = -1f;
        }
        if (float.IsNaN(carYNormalized))
        {
            Debug.Log("Car: carYNormalized == NaN");
            carYNormalized = -1f;
        }
        if (float.IsNaN(carZNormalized))
        {
            Debug.Log("Car: carZNormalized == NaN");
            carZNormalized = -1f;
        }
        sensor.AddObservation(new Vector3(carXNormalized, carYNormalized, carZNormalized));

        //Car rotation, already normalized
        float car_rotation_x = transform.rotation.x;
        float car_rotation_y = transform.rotation.y;
        float car_rotation_z = transform.rotation.z;
        float car_rotation_w = transform.rotation.w;
        if (float.IsNaN(car_rotation_x))
        {
            Debug.Log("Car: car_rotation_x == NaN");
            car_rotation_x = -1f;
        }
        if (float.IsNaN(car_rotation_y))
        {
            Debug.Log("Car: car_rotation_y == NaN");
            car_rotation_y = -1f;
        }
        if (float.IsNaN(car_rotation_z))
        {
            Debug.Log("Car: car_rotation_z == NaN");
            car_rotation_z = -1f;
        }
        if (float.IsNaN(car_rotation_w))
        {
            Debug.Log("Car: car_rotation_w == NaN");
            car_rotation_w = -1f;
        }
        sensor.AddObservation(new Quaternion(car_rotation_x, car_rotation_y, car_rotation_z, car_rotation_w));
        //Car velocity
        Vector3 car_velocity = _rb.velocity.normalized * (_rb.velocity.magnitude / 23f);
        if (float.IsNaN(car_velocity.x))
        {
            Debug.Log("Car: car_velocity.x == NaN");
            car_velocity.x = -1f;
        }
        if (float.IsNaN(car_velocity.y))
        {
            Debug.Log("Car: car_velocity.y == NaN");
            car_velocity.y = -1f;
        }
        if (float.IsNaN(car_velocity.z))
        {
            Debug.Log("Car: car_velocity.z == NaN");
            car_velocity.z = -1f;
        }
        sensor.AddObservation(car_velocity);

        //Car angular velocity
        Vector3 car_angularVelocity = _rb.angularVelocity.normalized * (_rb.angularVelocity.magnitude / 5.5f);
        if (float.IsNaN(car_angularVelocity.x))
        {
            Debug.Log("Car: _rb.angularVelocity.x == NaN");
            car_angularVelocity.x = -1f;
        }
        if (float.IsNaN(car_angularVelocity.y))
        {
            Debug.Log("Car: _rb.angularVelocity.y == NaN");
            car_angularVelocity.y = -1f;
        }
        if (float.IsNaN(car_angularVelocity.z))
        {
            Debug.Log("Car: _rb.angularVelocity.z == NaN");
            car_angularVelocity.z = -1f;
        }
        sensor.AddObservation(car_angularVelocity);

        //Ball position
        // TODO: Welche Normalisierung?? (alle /120f?)
        var ballXNormalized = (_ball.localPosition.x + 60f) / 120f;
        var ballYNormalized = _ball.localPosition.y / 20f;
        var ballZNormalized = (_ball.localPosition.z + 41f) / 82f;
        if (float.IsNaN(ballXNormalized))
        {
            Debug.Log("Ball: ballXNormalized == NaN");
            ballXNormalized = -1f;
        }
        if (float.IsNaN(ballYNormalized))
        {
            Debug.Log("Ball: ballYNormalized == NaN");
            ballYNormalized = -1f;
        }
        if (float.IsNaN(ballZNormalized))
        {
            Debug.Log("Ball: ballZNormalized == NaN");
            ballZNormalized = -1f;
        }
        sensor.AddObservation(new Vector3(ballXNormalized, ballYNormalized, ballZNormalized));

        //Ball velocity
        Vector3 ball_velocity = _rb.velocity.normalized * (_rbBall.velocity.magnitude / 60f);
        if (float.IsNaN(ball_velocity.x))
        {
            Debug.Log("Ball: ball_velocity.x == NaN");
            ball_velocity.x = -1f;
        }
        if (float.IsNaN(ball_velocity.y))
        {
            Debug.Log("Ball: ball_velocity.y == NaN");
            ball_velocity.y = -1f;
        }
        if (float.IsNaN(ball_velocity.z))
        {
            Debug.Log("Ball: ball_velocity.z == NaN");
            ball_velocity.z = -1f;
        }
        sensor.AddObservation(ball_velocity);

        // Boost amount
        float boostAmount = _boostControl._boostAmount / 100f;
        if (float.IsNaN(boostAmount))
        {
            Debug.Log("Car: boostAmount == NaN");
            boostAmount = -1f;
        }
        sensor.AddObservation(boostAmount);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (InputManager.isAgent)
        {
            switch (_actionSpaceType)
            {
                case ActionSpaceType.Continuous:
                    ProcessContinuousActions(actionBuffers);
                    break;
                case ActionSpaceType.MultiDiscrete:
                    ProcessMultiDiscreteActions(actionBuffers);
                    break;
                case ActionSpaceType.Mixed:
                    ProcessMixedActions(actionBuffers);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("The method {0} does not support the {1} '{2}'.", nameof(OnActionReceived), typeof(ActionSpaceType), _actionSpaceType.ToString()));
                    break;
            }
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
        _handler.ResetParameter();
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    private void AssignReward()
    {
        AddReward(-0.001f);
        
        if (_rbBall.velocity.x > 0 || Time.time - _lastResetTime > _episodeLength)
        {
            // Agent scored a goal
            SetReward(2f);

            Reset();
        }
        if (_mapData.isScoredOrange)
        {
            // Agent got scored on
            SetReward(-1f);
            Reset();
        }
    }

    /// <summary>
    /// Processes the actions, if <see cref="actionSpaceType"/> is <see cref="ActionSpaceType.Continuous"/>.
    /// </summary>
    /// <param name="actionBuffers">The action buffers containing the actions.</param>
    private void ProcessContinuousActions(ActionBuffers actionBuffers)
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

    /// <summary>
    /// Processes the actions, if <see cref="actionSpaceType"/> is <see cref="ActionSpaceType.MultiDiscrete"/>.
    /// </summary>
    /// <param name="actionBuffers">The action buffers containing the actions.</param>
    private void ProcessMultiDiscreteActions(ActionBuffers actionBuffers)
    {
        InputManager.throttleInput = DISCRETE_ACTIONS[actionBuffers.DiscreteActions[0]];
        InputManager.steerInput = DISCRETE_ACTIONS[actionBuffers.DiscreteActions[1]];
        InputManager.yawInput = DISCRETE_ACTIONS[actionBuffers.DiscreteActions[1]];
        InputManager.pitchInput = DISCRETE_ACTIONS[actionBuffers.DiscreteActions[2]];
        switch (actionBuffers.DiscreteActions[3])
        {
            case 0:
                InputManager.rollInput = -1;
                break;
            case 2:
                InputManager.rollInput = 1;
                break;
            default:
                InputManager.rollInput = 0;
                break;
        }
        InputManager.isBoost = actionBuffers.DiscreteActions[4] > 0;
        InputManager.isDrift = actionBuffers.DiscreteActions[5] > 0;
        InputManager.isAirRoll = actionBuffers.DiscreteActions[6] > 0;
        InputManager.isJump = actionBuffers.DiscreteActions[7] > 0;
    }

    /// <summary>
    /// Processes the actions, if <see cref="actionSpaceType"/> is <see cref="ActionSpaceType.Mixed"/>.
    /// </summary>
    /// <param name="actionBuffers">The action buffers containing the actions.</param>
    private void ProcessMixedActions(ActionBuffers actionBuffers)
    {
        //Continuous actions
        InputManager.throttleInput = actionBuffers.ContinuousActions[0];
        InputManager.steerInput = actionBuffers.ContinuousActions[1];
        InputManager.yawInput = actionBuffers.ContinuousActions[1];
        InputManager.pitchInput = actionBuffers.ContinuousActions[2];

        //Discrete actions
        switch (actionBuffers.DiscreteActions[0])
        {
            case 0:
                InputManager.rollInput = -1;
                break;
            case 2:
                InputManager.rollInput = 1;
                break;
            default:
                InputManager.rollInput = 0;
                break;
        }
        InputManager.isBoost = actionBuffers.DiscreteActions[1] > 0;
        InputManager.isDrift = actionBuffers.DiscreteActions[2] > 0;
        InputManager.isAirRoll = actionBuffers.DiscreteActions[3] > 0;
        InputManager.isJump = actionBuffers.DiscreteActions[4] > 0;
    }

    /// <summary>
    /// Determines the action space type for the specified <paramref name="actionSpec"/>.
    /// </summary>
    /// <param name="actionSpec">The <see cref="ActionSpec"/> for which we want to determine the action sapce type.</param>
    /// <returns>The corresponding action space type.</returns>
    private ActionSpaceType DetermineActionSpaceType(ActionSpec actionSpec)
    {
        int requiredNumActions = 8;

        // Determine action space type
        if(actionSpec.NumContinuousActions > 0 && actionSpec.NumDiscreteActions == 0)
        {
            //Propably continuous, we check the size
            if(actionSpec.NumContinuousActions != requiredNumActions)
            {
                throw new ArgumentException(string.Format("It seems like you tried to use a continuos action space for the agent. In this case the {0} needs 8 continuous actions.", typeof(GoalKeeperAgent)));
            }
            return ActionSpaceType.Continuous;
        }
        else if(actionSpec.NumContinuousActions == 0 && actionSpec.NumDiscreteActions > 0)
        {
            //Propably multi discrete, we check the size
            if (actionSpec.NumDiscreteActions != requiredNumActions)
            {
                throw new ArgumentException(string.Format("It seems like you tried to use a multi-discrete action space for the agent. In this case the {0} needs 8 discrete action branches.", typeof(GoalKeeperAgent)));
            }
            int[] requiredBranchSizes = { DISCRETE_ACTIONS.Length, DISCRETE_ACTIONS.Length, DISCRETE_ACTIONS.Length, 3, 2, 2, 2, 2};
            for(int i = 0; i < actionSpec.BranchSizes.Length; i++)
            {
                if(actionSpec.BranchSizes[i] != requiredBranchSizes[i])
                {
                    throw new ArgumentException(string.Format("It seems like you tried to use a multi-discrete action space for the agent. In this case the {0} needs 8 discrete action branches with sizes ({1}).", 
                        typeof(GoalKeeperAgent), 
                        string.Join(", ", requiredBranchSizes)));
                }
            }
            return ActionSpaceType.MultiDiscrete;
        }
        else
        {
            //Propably multi discrete, we check the size
            if (actionSpec.NumDiscreteActions != 5 || actionSpec.NumContinuousActions != 3)
            {
                throw new ArgumentException(string.Format("It seems like you tried to use a mixed action space for the agent. In this case the {0} needs 5 discrete action branches and 3 continuous actions.", typeof(GoalKeeperAgent)));
            }
            int[] requiredBranchSizes = { 3, 2, 2, 2, 2 };
            for (int i = 0; i < actionSpec.BranchSizes.Length; i++)
            {
                if (actionSpec.BranchSizes[i] != requiredBranchSizes[i])
                {
                    throw new ArgumentException(string.Format("It seems like you tried to use a mixed action space for the agent. In this case the {0} needs 5 discrete action branches with sizes ({1}).",
                        typeof(GoalKeeperAgent),
                        string.Join(", ", requiredBranchSizes)));
                }
            }
            return ActionSpaceType.Mixed;
        }
    }

    /// <summary>
    /// Helper for classifying the action space.
    /// </summary>
    private enum ActionSpaceType
    {
        Continuous,
        MultiDiscrete,
        Mixed
    }
}


