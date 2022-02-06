using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class OneVsOneAgentTest : Agent
{
    // Start is called before the first frame update

    private TeamController _teamController;
    private TeamController.Team _team;
    private MapData _mapData;

    private Rigidbody _rb, _rbBall, _rbEnemy;

    private float _episodeLength;
    private float _lastResetTime;

    private MatchEnvControllerTest _matchEnvController;

    private Transform _ball, _enemy;

    private CubeJumping _jumpControl;
    private CubeController _controller;
    private CubeBoosting _boostControl;
    private CubeGroundControl _groundControl;
    private CubeAirControl _airControl;

    public InputManager InputManager;

    private readonly float[] DISCRETE_ACTIONS = { -1f, -0.5f, 0f, 0.5f, 1f };
    private ActionSpaceType _actionSpaceType;

    private int _nBallTouches = 0;

    void Start()
    {
        _episodeLength = transform.parent.GetComponent<MatchTimeController>().matchTimeSeconds;

        _matchEnvController = transform.parent.GetComponent<MatchEnvControllerTest>();

        InputManager = GetComponent<InputManager>();
        InputManager.isAgent = true;

        _teamController = GetComponentInParent<TeamController>();
        _mapData = transform.parent.Find("World").Find("Rocket_Map").GetComponent<MapData>();

        _rb = GetComponent<Rigidbody>();
        _airControl = GetComponentInChildren<CubeAirControl>();
        _jumpControl = GetComponentInChildren<CubeJumping>();
        _controller = GetComponentInChildren<CubeController>();
        _boostControl = GetComponentInChildren<CubeBoosting>();
        _groundControl = GetComponentInChildren<CubeGroundControl>();

        _ball = transform.parent.Find("Ball");
        _rbBall = _ball.GetComponent<Rigidbody>();

        ActionSpec actionSpec = GetComponent<BehaviorParameters>().BrainParameters.ActionSpec;
        _actionSpaceType = DetermineActionSpaceType(actionSpec);
    }

    public void FixedUpdate()
    {
        AddReward(- (Time.fixedDeltaTime / _episodeLength));
    }

    public override void OnEpisodeBegin()
    {
        //Respawn Cars
        // transform.parent.GetComponent<TeamController>().SpawnTeams();

        //Define Enemy
        // if (gameObject.Equals(_teamController.TeamBlue[0]))
        // {
        //     _enemy = _teamController.TeamOrange[0].transform;
        //     _team = TeamController.Team.BLUE;
        // }
        // else
        // {
        //     _enemy = _teamController.TeamBlue[0].transform;
        //     _team = TeamController.Team.ORANGE;
        // }

        // _rbEnemy = _enemy.GetComponent<Rigidbody>();

        //Reset Ball
        // _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        // _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        _nBallTouches = 0;
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

        // Boost amount
        sensor.AddObservation(_boostControl._boostAmount / 100f);
        
        //Enemy position
        // var enemyXNormalized = (_enemy.localPosition.x + 60f) / 120f;
        // var enemyYNormalized = _enemy.localPosition.y / 20f;
        // var enemyZNormalized = (_enemy.localPosition.z + 41f) / 82f;
        // sensor.AddObservation(new Vector3(enemyXNormalized, enemyYNormalized, enemyZNormalized));
        // //Enemy rotation, already normalized
        // sensor.AddObservation(_enemy.rotation);
        // //Enemy velocity
        // sensor.AddObservation(_rbEnemy.velocity / 23f);
        // //Enemy angular velocity
        // sensor.AddObservation(_rbEnemy.angularVelocity / 5.5f);

        //Ball position
        // var ballXNormalized = (_ball.localPosition.x + 60f) / 120f;
        // var ballYNormalized = _ball.localPosition.y / 20f;
        // var ballZNormalized = (_ball.localPosition.z + 41f) / 82f;
        // sensor.AddObservation(new Vector3(ballXNormalized, ballYNormalized, ballZNormalized));
        // //Ball velocity
        // sensor.AddObservation(_rbBall.velocity / 60f);

        var localPosition = transform.localPosition;
        // var relativePositionToEnemy = (_enemy.localPosition - localPosition) / _mapData.diag;
        var relativePositionToBall = (_ball.localPosition - localPosition) / _mapData.diag;

        // sensor.AddObservation(relativePositionToEnemy);
        sensor.AddObservation(relativePositionToBall);
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

    private void AssignReward()
    {
        // if (_team.Equals(TeamController.Team.BLUE) && _rb.transform.localPosition.x > 0 ||
        //     _team.Equals(TeamController.Team.ORANGE) && _rb.transform.localPosition.x < 0)
        // {
        //     AddReward(0.001f);
        // }
        // else
        // {
        //     AddReward(-0.001f);
        // }
        
        //AddReward(0.001f * Mathf.Sign(Vector3.Dot(_ball.position - transform.position, _rb.velocity)));
        // AddReward(0.001f * Mathf.Sign(Vector3.Dot(_rbBall.position - transform.position, _rb.velocity)));

        // float agentBallDistanceReward = 0.001f * (1 - (Vector3.Distance(_ball.position, transform.position) / _mapData.diag));
        // AddReward(agentBallDistanceReward);
        
        // GameObject goalLines = transform.parent.Find("World").Find("Rocket_Map").Find("GoalLines").gameObject;
        // Vector3 enemyGoalPosition = _team.Equals(TeamController.Team.BLUE) ? goalLines.transform.Find("GoalLineRed").position : goalLines.transform.Find("GoalLineBlue").position;
        // float ballEnemyGoalDistanceReward = 0.001f * (1 - (Vector3.Distance(_ball.position, enemyGoalPosition) / _mapData.diag));
        // AddReward(ballEnemyGoalDistanceReward);
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag.Equals("Ball"))
        {
            int maxBallTouches = 1;
            AddReward(1.0f / maxBallTouches);
            // _enemy.GetComponent<OneVsOneAgent>().AddReward(-1.0f/maxBallTouches);
            ++_nBallTouches;
            if (_nBallTouches < maxBallTouches)
            {
                _matchEnvController.ResetBall();
            }
            else
            {
                _matchEnvController.Reset();
                _nBallTouches = 0;
            }
        }
    }

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
