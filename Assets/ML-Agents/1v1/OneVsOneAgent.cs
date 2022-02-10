using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;

public class OneVsOneAgent : PGBaseAgent
{
    // Start is called before the first frame update

    private TeamController _teamController;
    private TeamController.Team _team;

    private Rigidbody _rbBall, _rbEnemy;

    private float _episodeLength;
    private float _lastResetTime;

    private MatchEnvController _matchEnvController;

    private Transform _ball, _enemy;

    private int _nBallTouches = 0;

    void Start()
    {
        _episodeLength = transform.parent.GetComponent<MatchTimeController>().matchTimeSeconds;

        _matchEnvController = transform.parent.GetComponent<MatchEnvController>();
        _teamController = GetComponentInParent<TeamController>();


        _ball = transform.parent.Find("Ball");
        _rbBall = _ball.GetComponent<Rigidbody>();
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
        if (gameObject.Equals(_teamController.TeamBlue[0]))
        {
            _enemy = _teamController.TeamOrange[0].transform;
            _team = TeamController.Team.BLUE;
        }
        else
        {
            _enemy = _teamController.TeamBlue[0].transform;
            _team = TeamController.Team.ORANGE;
        }

        _rbEnemy = _enemy.GetComponent<Rigidbody>();

        //Reset Ball
        // _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        // _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car position
        Vector3 carPositionNormalized = NormalizeVec(rb,VectorType.Position,EntityType.Car);
        checkNormalizedVec(carPositionNormalized, "carPositionNormalized", VectorType.Position);
        sensor.AddObservation(carPositionNormalized);

        //Car rotation, already normalized
        sensor.AddObservation(transform.rotation);
        //Car velocity
        sensor.AddObservation(rb.velocity / 23f);
        //Car angular velocity
        sensor.AddObservation(rb.angularVelocity / 5.5f);

        // Boost amount
        sensor.AddObservation(boostControl.boostAmount / 100f);

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
        var relativePositionToEnemy = (_enemy.localPosition - localPosition) / mapData.diag;
        var relativePositionToBall = (_ball.localPosition - localPosition) / mapData.diag;

        sensor.AddObservation(relativePositionToEnemy);
        sensor.AddObservation(relativePositionToBall);
    }

    protected override void AssignReward()
    {
        // if (_team.Equals(TeamController.Team.BLUE) && rb.transform.localPosition.x > 0 ||
        //     _team.Equals(TeamController.Team.ORANGE) && rb.transform.localPosition.x < 0)
        // {
        //     AddReward(0.001f);
        // }
        // else
        // {
        //     AddReward(-0.001f);
        // }

        //AddReward(0.001f * Mathf.Sign(Vector3.Dot(_ball.position - transform.position, rb.velocity)));
        // AddReward(0.001f * Mathf.Sign(Vector3.Dot(_rbBall.position - transform.position, rb.velocity)));

        // float agentBallDistanceReward = 0.001f * (1 - (Vector3.Distance(_ball.position, transform.position) / mapData.diag));
        // AddReward(agentBallDistanceReward);

        // GameObject goalLines = transform.parent.Find("World").Find("Rocket_Map").Find("GoalLines").gameObject;
        // Vector3 enemyGoalPosition = _team.Equals(TeamController.Team.BLUE) ? goalLines.transform.Find("GoalLineRed").position : goalLines.transform.Find("GoalLineBlue").position;
        // float ballEnemyGoalDistanceReward = 0.001f * (1 - (Vector3.Distance(_ball.position, enemyGoalPosition) / mapData.diag));
        // AddReward(ballEnemyGoalDistanceReward);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Ball"))
        {
            int maxBallTouches = 3;
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
}
