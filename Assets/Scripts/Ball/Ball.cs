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
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }


}

