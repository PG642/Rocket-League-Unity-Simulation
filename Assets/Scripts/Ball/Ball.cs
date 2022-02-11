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
        Vector3 impactPosition = CalculateImpactPosition();

    }
    //Calculates the Spot the Ball would land on on the edges of the arena (including the ground and ceiling).
    private Vector3 CalculateImpactPosition()
    {
        float r =                               //Ball radius
            GetComponent<SphereCollider>().radius;               
        
        Vector3 V_hor = 
            new Vector3(rb.velocity.x, 0, rb.velocity.z);   
        Vector3 dir = V_hor.normalized;         //Top-down direction the ball is moving in
        float V_x = V_hor.magnitude;            //Velocity in dir
        float V_y = rb.velocity.y;              //Upwards velocity

        float G = -Physics.gravity.y;            //Gravity  

        float h_ceiling = 20.44f;               //Height of the arena ceiling
        float h = rb.position.y-r;              //Height above rest at ground                 
        float h_diff = -h;                      //Height Difference to impact point
        float h_max =                           //Max Height for uninterrupted flight
            (float)(h + Math.Pow(V_y, 2) / (2 * G));

        //Arena Wall Vectors
        Vector3 p1 = new Vector3(-39.68f, r, 40.96f);
        Vector3 p2 = new Vector3(51.2f, r, 29.44f);
        Vector3 p3 = new Vector3(39.68f, r, -40.96f);
        Vector3 p4 = new Vector3(-51.2f, r, -29.44f);

        Vector3 v1 = new Vector3(102.4f, 0, 0); //long walls
        Vector3 v2 = new Vector3(0, 0, 81.92f); //short walls
        Vector3 v3 = new Vector3(16.29f, 0, 16.29f); //corner walls 1 
        Vector3 v4 = new Vector3(-16.29f, 0, 16.29f); //corer walls 2


        float t_impact_ground = (float)((V_y + Math.Sqrt(Math.Pow(V_y, 2) - h * 2 * G)) / G);
        if (h_max >= (h_ceiling - r))           //Ball hits the ceiling
        {
            float t_impact_ceiling = (float)((V_y - Math.Sqrt(Math.Pow(V_y, 2) + (h_ceiling - r - rb.position.y) * 2 * G)) / G);
        }

        float dist = t_impact_ground * V_x;            //Distance the Ball travels in current direction until impact
        Vector3 p_impact_ground =                      //Impact Point (Center of Ball at impact)
            rb.position + dir * dist;    
        p_impact_ground.y = r;



        return p_impact_ground;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }


}

