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

    public TeamController teamController;
    public TeamController.Team team;

    public Rigidbody rbBall, rbEnemy;

    public MatchEnvController matchEnvController;

    public Transform ball, enemy;

    void Start()
    {
        base.Start();
        //_episodeLength = transform.parent.GetComponent<MatchTimeController>().matchTimeSeconds;

        matchEnvController = transform.parent.GetComponent<MatchEnvController>();
        teamController = GetComponentInParent<TeamController>();


        ball = transform.parent.Find("Ball");
        rbBall = ball.GetComponent<Rigidbody>();
    }


    public override void OnEpisodeBegin()
    {
        //Respawn Cars
        // transform.parent.GetComponent<TeamController>().SpawnTeams();

        //Define Enemy
        if (gameObject.Equals(teamController.TeamBlue[0]))
        {
            enemy = teamController.TeamOrange[0].transform;
            team = TeamController.Team.BLUE;
        }
        else
        {
            enemy = teamController.TeamBlue[0].transform;
            team = TeamController.Team.ORANGE;
        }

        rbEnemy = enemy.GetComponent<Rigidbody>();

        //Reset Ball
        // _ball.localPosition = new Vector3(Random.Range(-10f, 0f), Random.Range(0f, 20f), Random.Range(-30f, 30f));
        // _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Car self absolute -------------------------
        //Car position
        Vector3 carPositionNormalized = NormalizeVec(rb,VectorType.Position,EntityType.Car);
        checkNormalizedVec(carPositionNormalized, "carPositionNormalized", VectorType.Position);
        sensor.AddObservation(carPositionNormalized);

        //Car rotation, already normalized
        Quaternion carRotationNormalized = transform.localRotation;
        checkQuaternion(carRotationNormalized, "carRotationNormalized", 0);
        sensor.AddObservation(carRotationNormalized);

        //Car velocity
        Vector3 carVelocityNormalized = NormalizeVec(rb, VectorType.Velocity, EntityType.Car);
        checkNormalizedVec(carVelocityNormalized, "carVelocityNormalized", VectorType.Velocity);
        sensor.AddObservation(carVelocityNormalized);

        //Car angular velocity
        Vector3 carAngularVelocityNormalized = NormalizeVec(rb, VectorType.AngularVelocity, EntityType.Car);
        checkNormalizedVec(carAngularVelocityNormalized, "carAngularVelocityNormalized", VectorType.AngularVelocity);
        sensor.AddObservation(carAngularVelocityNormalized);

        //Car Boost amount
        sensor.AddObservation(boostControl.boostAmount / 100f);

        //Enemy Boost amount
        sensor.AddObservation(enemy.GetComponentInChildren<CubeBoosting>().boostAmount / 100f);

        //Enemy Absolute ------------
        //Enemy position
        Vector3 enemyPositionNormalized = NormalizeVec(rbEnemy, VectorType.Position, EntityType.Car);
        checkNormalizedVec(enemyPositionNormalized, "enemyPositionNormalized", VectorType.Position);
        sensor.AddObservation(enemyPositionNormalized);


        //Enemy rotation, already normalized
        Quaternion enemyRotationNormalized = enemy.localRotation;
        checkQuaternion(enemyRotationNormalized, "enemyRotationNormalized", 0);
        sensor.AddObservation(enemyRotationNormalized);

        //Enemy velocity
        Vector3 enemyVelocityNormalized = NormalizeVec(rbEnemy, VectorType.Velocity, EntityType.Car);
        checkNormalizedVec(enemyVelocityNormalized, "enemyVelocityNormalized", VectorType.Velocity);
        sensor.AddObservation(enemyVelocityNormalized);

        //Enemy angular velocity
        Vector3 enemyAngularVelocityNormalized = NormalizeVec(rbEnemy, VectorType.AngularVelocity, EntityType.Car);
        checkNormalizedVec(enemyAngularVelocityNormalized, "enemyAngularVelocityNormalized", VectorType.AngularVelocity);
        sensor.AddObservation(enemyAngularVelocityNormalized);

        

        //Ball absolute -----------
        //Ball position
        Vector3 ballPositionNormalized = NormalizeVec(rbBall, VectorType.Position, EntityType.Ball);
        checkNormalizedVec(ballPositionNormalized, "ballPositionNormalized", VectorType.Position);
        sensor.AddObservation(ballPositionNormalized);

        //Ball velocity
        Vector3 ballVelocityNormalized = NormalizeVec(rbBall, VectorType.Velocity, EntityType.Ball);
        checkNormalizedVec(ballVelocityNormalized, "ballVelocityNormalized", VectorType.Velocity);
        sensor.AddObservation(ballVelocityNormalized);

        //Ball angular velocity
        Vector3 ballAngularVelocityNormalized = NormalizeVec(rbBall, VectorType.AngularVelocity, EntityType.Ball);
        checkNormalizedVec(ballAngularVelocityNormalized, "ballAngularVelocityNormalized", VectorType.AngularVelocity);
        sensor.AddObservation(ballAngularVelocityNormalized);


        //Enemy Relative ----------------
        Vector3 enemyRelativePositionNormalized = (enemy.localPosition - transform.localPosition) / mapData.diag;
        sensor.AddObservation(enemyRelativePositionNormalized);

        Quaternion enemyRelativeRotationNormalized = Quaternion.RotateTowards(transform.localRotation, enemy.localRotation, 360f);
        sensor.AddObservation(enemyRelativeRotationNormalized);

        Vector3 enemyRelativeVelocityNormalized = (rbEnemy.velocity - rb.velocity) / 46f;
        sensor.AddObservation(enemyRelativeVelocityNormalized);

        Vector3 enemyRelativeAngularVelocityNormalized = (rbEnemy.angularVelocity - rb.angularVelocity) / 11f;
        sensor.AddObservation(enemyRelativeAngularVelocityNormalized);

        //Ball Relative --------------
        Vector3 ballRelativePositionNormalized = (ball.localPosition - transform.localPosition) / mapData.diag;
        sensor.AddObservation(ballRelativePositionNormalized);

        Vector3 ballRelativeVelocityNormalized = (rbBall.velocity - rb.velocity) / 83f;
        sensor.AddObservation(ballRelativeVelocityNormalized);

        Vector3 ballRelativeAngularVelocityNormalized = (rbBall.angularVelocity - rb.angularVelocity) / 11.5f;
        sensor.AddObservation(ballRelativeAngularVelocityNormalized);

        //Boost Pad Timers
        Transform boostpads = matchEnvController.transform.Find("World").Find("Rocket_Map").Find("Boostpads");
        foreach(Transform boostpad in boostpads)
        {
            sensor.AddObservation(boostpad.GetComponent<Boostpad>().remainingTime);
        }
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

}
