using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public abstract class PGBaseAgent : Agent
{

    /// <summary>
    /// Contains the possible values for the discretized actions.
    /// </summary>
    private readonly float[] DISCRETE_ACTIONS = { -1f, -0.5f, 0f, 0.5f, 1f };

    /// <summary>
    /// Shows whether the action space of the agent is continuous, multi-discrete or mixed.
    /// </summary>
    private ActionSpaceType _actionSpaceType;

    public InputManager InputManager;

    protected CubeJumping jumpControl;
    protected CubeController controller;
    protected CubeBoosting boostControl;
    protected CubeGroundControl groundControl;
    protected CubeAirControl airControl;
    protected Rigidbody rb;
    protected MapData mapData;

    protected abstract void AssignReward();

    // Start is called before the first frame update
    protected virtual void Start()
    {
        InputManager = GetComponent<InputManager>();
        InputManager.isAgent = true;

        ActionSpec actionSpec = GetComponent<BehaviorParameters>().BrainParameters.ActionSpec;
        _actionSpaceType = DetermineActionSpaceType(actionSpec);

        rb = GetComponent<Rigidbody>();
        airControl = GetComponentInChildren<CubeAirControl>();
        jumpControl = GetComponentInChildren<CubeJumping>();
        controller = GetComponentInChildren<CubeController>();
        boostControl = GetComponentInChildren<CubeBoosting>();
        groundControl = GetComponentInChildren<CubeGroundControl>();

        mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();
    }

    protected void AddRelativePositionNormalized(VectorSensor sensor, Transform otherTransform)
    {
        sensor.AddObservation((transform.localPosition - otherTransform.localPosition) / mapData.diag);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        InputManager.isAgent = false;
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
            }
        }
        AssignReward();
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
        if (actionSpec.NumContinuousActions > 0 && actionSpec.NumDiscreteActions == 0)
        {
            //Propably continuous, we check the size
            if (actionSpec.NumContinuousActions != requiredNumActions)
            {
                throw new ArgumentException(string.Format("It seems like you tried to use a continuos action space for the agent. In this case the {0} needs 8 continuous actions.", typeof(GoalKeeperAgent)));
            }
            return ActionSpaceType.Continuous;
        }
        else if (actionSpec.NumContinuousActions == 0 && actionSpec.NumDiscreteActions > 0)
        {
            //Propably multi discrete, we check the size
            if (actionSpec.NumDiscreteActions != requiredNumActions)
            {
                throw new ArgumentException(string.Format("It seems like you tried to use a multi-discrete action space for the agent. In this case the {0} needs 8 discrete action branches.", typeof(GoalKeeperAgent)));
            }
            int[] requiredBranchSizes = { DISCRETE_ACTIONS.Length, DISCRETE_ACTIONS.Length, DISCRETE_ACTIONS.Length, 3, 2, 2, 2, 2 };
            for (int i = 0; i < actionSpec.BranchSizes.Length; i++)
            {
                if (actionSpec.BranchSizes[i] != requiredBranchSizes[i])
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
    protected Vector3 NormalizeVec(Rigidbody rb, VectorType vecType, EntityType entity)
    {
        switch (vecType)
        {
            case VectorType.Position:
                return NormalizePosition(rb.transform);
            case VectorType.Velocity:
                return NormalizeVelocity(rb, entity);
            case VectorType.AngularVelocity:
                return NormalizeAngularVelocity(rb, entity);
            default:
                return Vector3.zero;
        }
    }

    protected Vector3 NormalizePosition(Transform objTransform)
    {
        var vec = new Vector3(objTransform.localPosition.x, objTransform.localPosition.y, objTransform.localPosition.z);

        vec.x = (vec.x + 60f) / 120f;
        vec.y = vec.y / 20f;
        vec.z = (vec.z + 41f) / 82f;
        return vec;
    }


    protected Vector3 NormalizeVelocity(Rigidbody rb, EntityType entity)
    {
        switch (entity)
        {
            case EntityType.Ball:
                return rb.velocity.normalized * (rb.velocity.magnitude / 60f);
            case EntityType.Car:
                return rb.velocity.normalized * (rb.velocity.magnitude / 23f);
            default:
                return Vector3.zero;
        }
    }

    protected Vector3 NormalizeAngularVelocity(Rigidbody rb, EntityType entity)
    {
        switch (entity)
        {
            case EntityType.Ball:
                return rb.angularVelocity.normalized * (rb.angularVelocity.magnitude / 6f);
            case EntityType.Car:
                return rb.angularVelocity.normalized * (rb.angularVelocity.magnitude / 5.5f);
            default:
                return Vector3.zero;
        }
    }

    protected bool checkNormalizedVec(Vector3 vec, string name, VectorType type)
    {
        switch (type)
        {
            case VectorType.Position:
                return checkVec(vec, name, -1f, true, false, 0f, 1f);
            case VectorType.Velocity:
                return checkVec(vec, name, -1f, false, true, -1f, 1f, 0f, 1f);
            case VectorType.AngularVelocity:
                return checkVec(vec, name, -1f, false, true, -1f, 1f, 0f, 1f);
            default:
                return true;
        }
    }


    protected bool checkVec(Vector3 vec, string name, float defaultValue, bool legalValueInterval=true, bool legalMagnitudeInterval = true, float minLegalValue=-5f, float maxLegalValue=5f, float minLegalMagnitude=-1f, float maxLegalMagnitude=5f)
    {
        bool reset = false;
        for (int i = 0; i < 3; i++)
        {
            if (float.IsNaN(vec[i]))
            {
                Debug.Log($"{name}[{i}] is NaN");
                vec[i] = defaultValue;
            }
            if (float.IsInfinity(vec[i]))
            {
                Debug.Log($"{name}[{i}] is Infinity");
                vec[i] = defaultValue;
            }
            if (legalValueInterval && (vec[i] < minLegalValue || vec[i] > maxLegalValue))
            {
                Debug.LogWarning($"{name}[{i}] is {vec[i]}, was expected to be in interval [{minLegalValue}, {maxLegalValue}]");
                reset = true;
            }
        }
        if (legalMagnitudeInterval && (vec.magnitude < minLegalMagnitude || vec.magnitude > maxLegalMagnitude))
        {
            Debug.LogWarning($"{name}.magnitude is {vec.magnitude}, was expected to be in interval [{minLegalMagnitude}, {maxLegalMagnitude}]");
            reset = true;
        }
        return reset;
    }

    protected void checkQuaternion(Quaternion quat, string name, float defaultValue)
    {
        for (int i = 0; i < 4; i++)
        {
            if (float.IsNaN(quat[i]))
            {
                Debug.Log(name + "[" + i + "] is NaN");
                quat[i] = defaultValue;
            }
            if (float.IsInfinity(quat[i]))
            {
                Debug.Log(name + "[" + i + "] is Infinity");
                quat[i] = defaultValue;
            }
        }
    }

    /// <summary>
    /// Helper for classifying the action space.
    /// </summary>
    public enum ActionSpaceType
    {
        Continuous,
        MultiDiscrete,
        Mixed
    }

    public enum VectorType
    {
        Position,
        Velocity,
        AngularVelocity
    }

    public enum EntityType
    {
        Ball,
        Car
    }
}
