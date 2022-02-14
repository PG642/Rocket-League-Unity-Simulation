using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : Resettable
{
    private const float MinVelocity = 0.4f;
    private const float MinAngularVelocity = 1.047f;
    private const float Restitution = 0.6f;
    private const float TimeWindowToStop = 2.0f;
    private const float Friction = 2f;
    private const float Mu = 0.285f;
    private const float A = 3f;
    private const float Radius = 0.9125f; //0.9138625f;

    private float _lastStoppedTime;
    private Transform _transform;

    public bool disableCustomBounce;
    public bool disableBulletImpulse;
    public bool disablePsyonixImpulse;
    public bool isTouchedGround;
    public bool stopSlowBall = true;
    public float maxAngularVelocity = 6.0f;
    public float maxVelocity = 60.0f;
    public AnimationCurve pysionixImpulseCurve = new AnimationCurve();

    public bool BallStuck;
    public bool useShotPrediction = false;
    public GameObject HitMarker;

    public override void Start()
    {
        base.Start();
        _transform = transform;
        isTouchedGround = false;
        rb.maxAngularVelocity = maxAngularVelocity;
        rb.maxDepenetrationVelocity = maxVelocity;
        BallStuck = false;
    }

    private void LateUpdate()
    {
        CapVelocities();

        StopBallIfTooSlow();
    }

    private void CapVelocities()
    {
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        if (rb.angularVelocity.magnitude > maxAngularVelocity)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularVelocity;
        }
    }

    [ContextMenu("ResetBall")]
    public void ResetBall()
    {
        var desired = new Vector3(0, 12.23f, 0f);
        _transform.localPosition = desired;
        _transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ShootInRandomDirection")]
    private void ShootInRandomDirection(float speed)
    {
        float speedRange = Random.Range(speed - 10, speed + 10);
        var randomDirection = Random.insideUnitCircle.normalized;
        var direction = new Vector3(randomDirection.x, Random.Range(-0.5f, 0.5f), randomDirection.y).normalized;
        rb.velocity = direction * speedRange;
    }

    private void StopBallIfTooSlow()
    {
        if (!stopSlowBall)
            return;

        if (rb.velocity.magnitude <= MinVelocity && rb.angularVelocity.magnitude <= MinAngularVelocity)
        {
            if (_lastStoppedTime == 0.0f)
            {
                _lastStoppedTime = Time.time;
            }

            if (_lastStoppedTime < Time.time - TimeWindowToStop && _lastStoppedTime > 0.0f)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            _lastStoppedTime = 0.0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //Debug.Log("Bounce");
            ApplyBounce(collision);
            isTouchedGround = true;
        }
        else
        {
            //Debug.Log("Hit");
            PerformPlayerHit(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = true;
        }
    }

    private void ApplyBounce(Collision col)
    {
        if (disableCustomBounce)
        {
            return;
        }
        Vector3 n = col.GetContact(0).normal;
        CancelUnityImpulse();
        if (rb.velocity.magnitude <= 1e-4f)
        {
            return;
        }

        Vector3 vPerp = Vector3.Dot(rb.velocity, n) * n;
        Vector3 vPara = rb.velocity - vPerp;
        Vector3 vSpin = Radius * Vector3.Cross(n, rb.angularVelocity);
        Vector3 s = vPara + vSpin;

        float ratio = s.magnitude <= 1e-4f ? 0.0f : vPerp.magnitude / s.magnitude;

        Vector3 deltaVPerp = -(1 + Restitution) * vPerp;
        Vector3 deltaVPara = -Math.Min(1, Friction * ratio) * Mu * s;

        rb.velocity += deltaVPara + deltaVPerp;
        rb.angularVelocity += A * Radius * Vector3.Cross(deltaVPara, n);
        PredictShot();
    }

    private void PerformPlayerHit(Collision col)
    {
        if (!disableBulletImpulse)
        {
            CancelUnityImpulse();
            col.gameObject.GetComponent<Resettable>().CancelUnityImpulse();
            //setCarState(col.rigidbody);
            Vector3 collisionPoint = col.rigidbody.ClosestPointOnBounds(rb.position); // col.GetContact(0).point;
            Nullable<Vector3> jBulletResult = -CustomPhysics.CalculateBulletImpulse(rb, col.rigidbody, collisionPoint);
            Vector3 jBullet;
            if (jBulletResult.HasValue)
            {
                jBullet = jBulletResult.Value;
            }
            else
            {
                BallStuck = true;
                jBullet = new Vector3(0f, 0f, 0f);
            }
            CustomPhysics.ApplyImpulseAtPosition(rb, jBullet, collisionPoint);
            CustomPhysics.ApplyImpulseAtPosition(col.rigidbody, -jBullet, collisionPoint);
        }
        if (!disablePsyonixImpulse)
        {
            Vector3 jPsyonix = CustomPhysics.CalculatePsyonixImpulse(rb, col, pysionixImpulseCurve);
            CustomPhysics.ApplyImpulseAtPosition(rb, jPsyonix, rb.position);
        }
        CapVelocities();
        if (col.rigidbody.velocity.magnitude > 23.00f)
        {
            col.rigidbody.velocity = col.rigidbody.velocity.normalized * 23.00f;
        }
        if (col.rigidbody.angularVelocity.magnitude > col.rigidbody.maxAngularVelocity)
        {
            col.rigidbody.angularVelocity = col.rigidbody.angularVelocity.normalized * col.rigidbody.maxAngularVelocity;
        }
        PredictShot();

    }

    private void PredictShot()
    {
        if (useShotPrediction)
        {
            Vector3 impactPosition = CalculateImpactPosition();
            if (HitMarker != null)
            {
                HitMarker.transform.position = impactPosition;
            }
        }
    }

    //Calculates the Spot the Ball would land on on the edges of the arena (including the ground and ceiling).
    private Vector3 CalculateImpactPosition()
    {
        float r =                               //Ball radius
            GetComponentInChildren<SphereCollider>().radius;
        float tolerance = 0.02f;
        float r_tol = r - tolerance;

        Vector3 V_hor =
            new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 dir = V_hor.normalized;         //Top-down direction the ball is moving in
        float V_x = V_hor.magnitude;            //Velocity in dir
        float V_y = rb.velocity.y;              //Upwards velocity

        float G = -Physics.gravity.y;           //Gravity  
        float h = transform.localPosition.y - r;//Height above rest at ground      
        float h_max =                           //Max Height for unhindered flight
            (float)(h + Math.Pow(V_y, 2) / (2 * G));

        float dist_ground = (float)(V_x * (V_y + Math.Sqrt(Math.Pow(V_y, 2) + h * 2 * G)) / G);  //Distance the Ball travels in current direction until impact
        Vector3 line_ground = dir * dist_ground;
        Vector3 ballPosition2d = new Vector3(transform.localPosition.x, r, transform.localPosition.z);
        Vector3 p_impact_ground = ballPosition2d + line_ground;  //Impact Point (Center of Ball at impact) if the ball flies unhindered (no walls or ceiling in the trajectory)
        Vector3 p_impact = p_impact_ground;

        //Arena Corners (out of ball perspective)
        Vector3[] arenaCorners = new[] {
            //Right wall
            new Vector3(-51.20f + r_tol, r,  29.44f - r_tol),
            new Vector3(-39.68f + r_tol, r,  40.96f - r_tol),
            new Vector3( 39.68f - r_tol, r,  40.96f - r_tol),
            new Vector3( 51.20f - r_tol, r,  29.44f - r_tol),

            //Left wall
            new Vector3( 51.20f - r_tol, r, -29.44f + r_tol),
            new Vector3( 39.68f - r_tol, r, -40.96f + r_tol),
            new Vector3(-39.68f + r_tol, r, -40.96f + r_tol),
            new Vector3(-51.20f + r_tol, r, -29.44f + r_tol),
            };

        Vector3[] goalCornersOrange = new[] {
            //Orange goal
            new Vector3( 51.20f - r_tol, r,   8.93f - r_tol),
            new Vector3( 60.00f - r_tol, r,   8.93f - r_tol),
            new Vector3( 60.00f - r_tol, r,  -8.93f + r_tol),
            new Vector3( 51.20f - r_tol, r,  -8.93f + r_tol),
            };

        Vector3[] goalCornersBlue = new[] {
            //Blue goal
            new Vector3(-51.20f + r_tol, r,  -8.93f + r_tol),
            new Vector3(-60.00f + r_tol, r,  -8.93f + r_tol),
            new Vector3(-60.00f + r_tol, r,   8.93f - r_tol),
            new Vector3(-51.20f + r_tol, r,   8.93f - r_tol),
            };

        float h_goal = 6.42775f - r_tol; 

        Vector3 intersection;
        Vector3 p_intersect_wall = Vector3.zero;

        for (int i = 0; i < arenaCorners.Length; i++)
        {
            if (CustomPhysics.LineSegmentIntersection2D(out intersection, arenaCorners[i], arenaCorners[(i+1)% arenaCorners.Length], p_impact_ground, ballPosition2d))
            {
                p_intersect_wall = intersection;
            }
        }

        bool wallHit = p_intersect_wall != Vector3.zero;
        float dist_wall = (p_intersect_wall - ballPosition2d).magnitude;

        //Height of the arena ceiling
        float h_ceiling = 20.44f - r_tol;           
        bool ceilingHit = h_max >= h_ceiling;
        float dist_ceiling = (float)(V_x * (V_y - Math.Sqrt(Math.Pow(V_y, 2) - (h_ceiling - transform.localPosition.y) * 2 * G)) / G);


        //Trajectory intercepts both wall and ceiling, determine what gets hit first
        if (wallHit && ceilingHit)
        {
            if(dist_wall < dist_ceiling)
            {
                ceilingHit = false;
            }
            else
            {
                wallHit = false;
            }
        }
        if (wallHit)
        {
            p_impact = transform.localPosition + dir * dist_wall;
            p_impact.y = (float)(h + (dist_wall * V_y / V_x) - (Math.Pow(dist_wall, 2) * G) / (2 * Math.Pow(V_x, 2)));

            //Shot into Goal?
            if(p_impact.y <= h_goal && Math.Abs(p_impact.z)<= 8.93f - r_tol)
            {
                intersection = Vector3.zero;
                p_intersect_wall = Vector3.zero;
                Vector3[] goalCorners = p_impact.x > 0 ? goalCornersOrange : goalCornersBlue;
                for (int i = 0; i < goalCorners.Length-1; i++)
                {
                    if (CustomPhysics.LineSegmentIntersection2D(out intersection, goalCorners[i], goalCorners[i + 1], p_impact_ground, ballPosition2d))
                    {
                        p_intersect_wall = intersection;
                    }
                }
                wallHit = p_intersect_wall != Vector3.zero;
                dist_wall = (p_intersect_wall - ballPosition2d).magnitude;
                if (wallHit)
                {
                    p_impact = transform.localPosition + dir * dist_wall;
                    p_impact.y = (float)(h + (dist_wall * V_y / V_x) - (Math.Pow(dist_wall, 2) * G) / (2 * Math.Pow(V_x, 2)));
                    ceilingHit = h_max >= h_goal;
                    dist_ceiling = (float)(V_x * (V_y - Math.Sqrt(Math.Pow(V_y, 2) - (h_goal - transform.localPosition.y) * 2 * G)) / G);
                    h_ceiling = h_goal;
                }
                else
                {
                    p_impact = p_impact_ground;
                }
            }
        }
        if (ceilingHit)
        {
            p_impact = transform.localPosition + dir * dist_ceiling;
            p_impact.y = h_ceiling;
        }
        return p_impact;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }


}

