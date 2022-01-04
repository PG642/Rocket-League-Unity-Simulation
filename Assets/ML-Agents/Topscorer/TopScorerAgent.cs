using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public class TopScorerAgent : Agent
{
    // Start is called before the first frame update

    private Rigidbody _rb, _rbBall;

    private float _episodeLength = 5f;
    private float _lastResetTime;

    private Transform _ball, _shootAt;
    private Vector3 _midFieldPosition;


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

        _midFieldPosition = Vector3.zero;
        _shootAt = transform.parent.Find("ShootAt");
        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();

        _lastResetTime = Time.time;
    }

    public override void OnEpisodeBegin()
    {
        Vector3 startPosition = _midFieldPosition + new Vector3(25f, 0.17f, UnityEngine.Random.Range(-15f, 15f));
        _controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f, 0f), 100f);


        //Reset Ball
        float ball_z_pos = UnityEngine.Random.Range(0, 9)%2==0 ? 6f : -6f;
        _ball.localPosition = new Vector3(45f, UnityEngine.Random.Range(2f, 5f), ball_z_pos + UnityEngine.Random.Range(-1f, 1f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(_ball.localPosition.x, 0f, 0f);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();

        _mapData.ResetIsScored();
        /*
        //Reset Car
        Vector3 startPosition = _midFieldPosition + new Vector3(0f, 0.17f, UnityEngine.Random.Range(-15f, 15f));

        _controller.ResetCar(startPosition, Quaternion.Euler(0f, 90f + UnityEngine.Random.Range(-20f, 20f), 0f), 100f);

        //Reset Ball
        _ball.localPosition = new Vector3(UnityEngine.Random.Range(30f, 50f), UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(-20f, 20f));
        //_ball.rotation = Quaternion.Euler(0f, 0f, 0f);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set new Taget Position
        _shootAt.localPosition = new Vector3(UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(0f, 7f), _rb.position.z);

        //Throw Ball
        _ball.GetComponent<ShootBall>().ShootTarget();
        _mapData.ResetIsScored();
        */

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
    }

    /// <summary>
    /// Assigns a reward if the maximum episode length is reached or a goal is scored by any team.
    /// </summary>
    private void AssignReward()
    {
        if (Time.time - _lastResetTime > _episodeLength)
        {
            // Agent didn't score a goal
            SetReward(-1f);
            Reset();
        }
        if (_mapData.isScoredBlue)
        {
            // Agent scored a goal
            SetReward(1f);
            // Add reward for scoring fast
            AddShortEpisodeReward(0.2f);
            Reset();
        }
    }

    private void AddShortEpisodeReward(float factor)
    {
        // adds a reward in range [0, factor]
        if (Time.time - _lastResetTime > _episodeLength)
        {
            return;
        }
        AddReward((1 - (Time.time - _lastResetTime / _episodeLength)) * factor);
    }

    private void AddFastShotReward(float factor)
    {
        // adds a reward in range [0, factor]
        if (Time.time - _lastResetTime > _episodeLength)
        {
            return;
        }
        AddReward((_rbBall.velocity.magnitude / _ball.GetComponent<Ball>().maxVelocity) * factor);
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
