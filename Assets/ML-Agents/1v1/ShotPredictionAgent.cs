using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotPredictionAgent : OneVsOneAgent
{
    float successfulSaveReward = 0f;
    float lastFrameBallHit = 0f;
    int ballHitsInLastPeriod = 0;
    Vector3[] enemyGoalCorners;
    const float maxBallGoalDist = 99.16083715f;
    const float maxCarBallDist = 112.9899243f;
    Collider enemyGoal;
    float r;
    Transform goalLines;

    // Start is called before the first frame update
    public void Init()
    {

        matchEnvController = transform.parent.GetComponent<MatchEnvController>();
        teamController = GetComponentInParent<TeamController>();


        ball = transform.parent.Find("Ball");
        rbBall = ball.GetComponent<Rigidbody>();
        ball.GetComponent<Ball>().useShotPrediction = true;
        ball.GetComponent<Ball>().agents.Add(this);

        r = ball.GetComponentInChildren<SphereCollider>().radius;
        goalLines = matchEnvController.transform.Find("World").Find("Rocket_Map").Find("GoalLines");
    }

    public void BallPrediction(Vector3 predictedImpact)
    {
        float r = ball.GetComponentInChildren<SphereCollider>().radius;
        float tolerance = 0.02f;
        float r_tol = r - tolerance;
        float h_goal = 6.42775f - r_tol;
        Vector3 orangeGoalCenter = new Vector3(51.20f, r, 0);
        Vector3 blueGoalCenter = new Vector3(-51.20f, r, 0);
        float goalAreaRadius = 2 * (8.93f - r);
        float shotOnGoalReward = 10f;
        float saveReward = 50f;
        float reward = 0;

        switch (team)
        {
            case TeamController.Team.BLUE:
                //Ball would land in enemy goal
                if(predictedImpact.x >= orangeGoalCenter.x)
                {
                    reward = shotOnGoalReward;
                    break;
                }
                //Ball would land in own goal
                if (predictedImpact.x <= blueGoalCenter.x)
                {
                    successfulSaveReward = saveReward;
                    break;
                }
                //Ball would land near enemy goal
                if ((ball.localPosition - orangeGoalCenter).magnitude > goalAreaRadius && (predictedImpact - orangeGoalCenter).magnitude <= goalAreaRadius && predictedImpact.y <= r + tolerance)
                {
                    reward = shotOnGoalReward * (goalAreaRadius - (predictedImpact - orangeGoalCenter).magnitude) / (2 * goalAreaRadius);
                    break;
                }
                //Ball would land near own goal
                if ((ball.localPosition - blueGoalCenter).magnitude > goalAreaRadius && (predictedImpact - blueGoalCenter).magnitude <= goalAreaRadius)
                {
                    successfulSaveReward = saveReward * (goalAreaRadius - (predictedImpact - blueGoalCenter).magnitude) / (2 * goalAreaRadius);
                    break;
                }
                //Ball was saved
                if ((ball.localPosition - blueGoalCenter).magnitude <= goalAreaRadius && (predictedImpact - blueGoalCenter).magnitude > goalAreaRadius)
                {
                    reward = successfulSaveReward;
                }
                successfulSaveReward = 0;
                break;

            case TeamController.Team.ORANGE:
                //Ball would land in enemy goal
                if (predictedImpact.x <= blueGoalCenter.x)
                {
                    reward = shotOnGoalReward;
                    break;
                }
                //Ball would land in own goal
                if (predictedImpact.x >= orangeGoalCenter.x)
                {
                    successfulSaveReward = saveReward;
                    break;
                }

                //Ball would land near enemy goal
                if ((ball.localPosition - blueGoalCenter).magnitude > goalAreaRadius && (predictedImpact - blueGoalCenter).magnitude <= goalAreaRadius && predictedImpact.y <= r + tolerance)
                {
                    reward = shotOnGoalReward * (goalAreaRadius - (predictedImpact - blueGoalCenter).magnitude) / (2 * goalAreaRadius);
                    break;
                }
                //Ball would land near own goal
                if ((ball.localPosition - orangeGoalCenter).magnitude > goalAreaRadius && (predictedImpact - orangeGoalCenter).magnitude <= goalAreaRadius)
                {
                    successfulSaveReward = saveReward * (goalAreaRadius - (predictedImpact - orangeGoalCenter).magnitude) / (2 * goalAreaRadius);
                    break;
                }
                //Ball was saved
                if ((ball.localPosition - orangeGoalCenter).magnitude <= goalAreaRadius && (predictedImpact - orangeGoalCenter).magnitude > goalAreaRadius)
                {
                    reward = successfulSaveReward;
                }
                successfulSaveReward = 0;
                break;

            default:
                Debug.Log("undefined team for shotprediction");
                break;
        }
        AddReward(reward);
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        float timePenalty = 0.01f;
        float angleScale = 1f;
        float ballGoalScale = 2f;

        Vector3 carToBall = ball.localPosition - transform.localPosition;
        float carToBallDist = carToBall.magnitude;
        Vector3 enemyGoalPoint = enemyGoal.ClosestPoint(ball.localPosition);
        enemyGoalPoint.x += team == TeamController.Team.BLUE ? 0.2f : -0.2f;
        Vector3 ballToGoal = enemyGoalPoint - ball.localPosition;
        float ballToGoalDist = ballToGoal.magnitude;
        int raycastMask = team == TeamController.Team.BLUE ? (1 << 14) : (1 << 13);
        bool ballAndCarAligned = Physics.Raycast(ball.localPosition, carToBall, maxBallGoalDist, raycastMask, QueryTriggerInteraction.Collide);
        float ballGoalAlignmentAngle = 0;
        if (!ballAndCarAligned)
        {
            
            Vector3[] carToGoalCorners = new Vector3[4];
            Plane[] carGoalPlanes = new Plane[4];
            for (int i = 0; i < 4; i++)
            {
                carToGoalCorners[i] = enemyGoalCorners[i] - transform.localPosition;
                /*
                if (team == TeamController.Team.BLUE)
                {
                    Debug.DrawLine(transform.localPosition, enemyGoalCorners[i], Color.red, 0.01f);
                }
                */
            }
            Ray ray = new Ray(ball.localPosition, ballToGoal);
            Vector3 intersection = Vector3.zero;
            float rayDist;
            for (int i = 0; i < 4; i++)
            {
                carGoalPlanes[i] = new Plane(Vector3.Cross(carToGoalCorners[i], carToGoalCorners[(i + 1) % 4]).normalized, transform.localPosition);
                /*
                if(team == TeamController.Team.BLUE)
                {
                    Debug.DrawLine(transform.localPosition, transform.localPosition + Vector3.Cross(carToGoalCorners[i], carToGoalCorners[(i + 1) % 4]).normalized, Color.black, 0.01f);
                }
                */
                if (carGoalPlanes[i].Raycast(ray, out rayDist))
                {
                    intersection = ray.GetPoint(rayDist);
                    if(CustomPhysics.PointInsidePositiveQuadrantOfPlane(carToGoalCorners[i], carToGoalCorners[(i + 1) % 4], transform.localPosition, intersection))
                    {
                        break;
                    }
                }
            }
            /*
            if (team == TeamController.Team.BLUE)
            {
                Debug.DrawLine(transform.localPosition, intersection, Color.green, 0.01f);
                Debug.DrawLine(transform.localPosition, ball.localPosition, Color.blue, 0.01f);
                Debug.DrawLine(ball.localPosition, enemyGoalPoint, Color.yellow, 0.01f);
            }
            */
            Vector3 carToIntersection = intersection - transform.localPosition;
            ballGoalAlignmentAngle = Vector3.Angle(carToIntersection, carToBall);
        }
        float penalty = carToBallDist + ballGoalScale * ballToGoalDist + angleScale * ballGoalAlignmentAngle;
        float maxPenalty = maxCarBallDist + ballGoalScale * maxBallGoalDist + angleScale * 180f;
        AddReward(-timePenalty * penalty/maxPenalty);

        /*
        if (team == TeamController.Team.BLUE)
        {
            Debug.Log("Angle: " + ballGoalAlignmentAngle);
            Debug.Log("Reward: " + (-timePenalty * penalty / maxPenalty));
        }
        */
    }


    public void InitializeEnemyGoal()
    {
        float teamFactorX = team == TeamController.Team.BLUE ? 1f : -1f;
        enemyGoalCorners = new Vector3[4];
        enemyGoalCorners[0] = new Vector3(teamFactorX * 51.2f, r, -8.92755f + r);
        enemyGoalCorners[1] = new Vector3(teamFactorX * 51.2f, 6.42775f - r, -8.92755f + r);
        enemyGoalCorners[2] = new Vector3(teamFactorX * 51.2f, 6.42775f - r, 8.92755f - r);
        enemyGoalCorners[3] = new Vector3(teamFactorX * 51.2f, r, 8.92755f - r);
        enemyGoal = team == TeamController.Team.BLUE ? goalLines.Find("GoalLineRed").GetComponent<Collider>() : goalLines.Find("GoalLineBlue").GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Ball"))
        {
            if (Time.frameCount - lastFrameBallHit >= 10f * 120f)
            {
                AddReward(2f);
                lastFrameBallHit = Time.frameCount;
                ballHitsInLastPeriod=1;
            }
            else
            {
                AddReward((float) Math.Pow(0.5, ballHitsInLastPeriod));
                ballHitsInLastPeriod++;
            }
        }
    }

}
