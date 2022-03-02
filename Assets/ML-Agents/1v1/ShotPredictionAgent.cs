using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotPredictionAgent : OneVsOneAgent
{
    float successfulSaveReward = 0f;
    float lastFrameBallHit = 0f;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        //_episodeLength = transform.parent.GetComponent<MatchTimeController>().matchTimeSeconds;

        matchEnvController = transform.parent.GetComponent<MatchEnvController>();
        teamController = GetComponentInParent<TeamController>();


        ball = transform.parent.Find("Ball");
        rbBall = ball.GetComponent<Rigidbody>();
        ball.GetComponent<Ball>().useShotPrediction = true;
        ball.GetComponent<Ball>().agent = this;
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
                if ((ball.localPosition - orangeGoalCenter).magnitude > goalAreaRadius && (predictedImpact - orangeGoalCenter).magnitude <= goalAreaRadius)
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
                if ((ball.localPosition - blueGoalCenter).magnitude > goalAreaRadius && (predictedImpact - blueGoalCenter).magnitude <= goalAreaRadius)
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
        AddReward(-0.001f);
    }

    public override void OnEpisodeBegin()
    {
        //Respawn Cars
        transform.parent.GetComponent<TeamController>().SpawnTeams();

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
        ball.localPosition = new Vector3(0, ball.GetComponentInChildren<SphereCollider>().radius, 0);
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Ball"))
        {
            if (Time.frameCount - lastFrameBallHit >= 10f * 120f)
            {
                AddReward(2f);
                lastFrameBallHit = Time.frameCount;
            }
        }
    }

}
