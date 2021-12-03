using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : Resettable
{
    [SerializeField] [Range(10, 80)] float randomSpeed = 40;
    [SerializeField] float initialForce;
    [SerializeField] float hitMultiplier;
    public float maxAngluarVelocity = 6.0f;
    public float maxVelocity = 60.0f;
    public AnimationCurve pysionixImpulseCurve = new AnimationCurve();
    public bool isTouchedGround = false;
    private const float MINVelocity = 0.4f;
    private const float MINAngularVelocity = 1.047f;
    private float _lastStoppedTime;
    private const float Restitution = 0.6f;
    private const float TimeWindowToStop = 2.0f;
    private const float Friction = 2f;
    private const float Mu = 0.285f;
    private const float A = 3f;
    private const float Radius = 0.9315f;
    private Transform _transform;

    public override void Start()
    {
        base.Start();
        _transform = this.transform;
        isTouchedGround = false;
        rb.maxAngularVelocity = maxAngluarVelocity;
        rb.maxDepenetrationVelocity = maxVelocity;
    }

    // void Update()
    // {
    //    if (Input.GetKeyDown(KeyCode.T))
    //         ShootInRandomDirection(randomSpeed);
    //
    //     if (Input.GetKeyDown(KeyCode.R))
    //         ResetBall();
    //
    //     if (Input.GetButtonDown("Select"))
    //         ResetShot(new Vector3(7.76f, 2.98f, 0f));
    //     rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
    // }

    private void LateUpdate()
    {
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        if (rb.angularVelocity.magnitude > maxAngluarVelocity)
        {
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngluarVelocity;
        }

        StopBallIfTooSlow();
    }

    private void ResetShot(Vector3 pos)
    {
        _transform.position = pos;
        rb.velocity = new Vector3(30, 10, 0);
        rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ResetBall")]
    public void ResetBall()
    {
        var desired = new Vector3(0, 12.23f, 0f);
        _transform.SetPositionAndRotation(desired, Quaternion.identity);
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
        if (rb.velocity.magnitude <= MINVelocity && rb.angularVelocity.magnitude <= MINAngularVelocity)
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
            Debug.Log("Bounce");
            ApplyBounce(collision);
            isTouchedGround = true;
        }
        else
        {
            Debug.Log("Hit");
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
        Vector3 n = col.GetContact(0).normal;
        CancelUnityImpulse();

        Vector3 vPerp = Vector3.Dot(rb.velocity, n) * n;
        Vector3 vPara = rb.velocity - vPerp;
        Vector3 vSpin = Radius * Vector3.Cross(n, rb.angularVelocity);
        Vector3 s = vPara + vSpin;

        float ratio = vPerp.magnitude / s.magnitude;

        Vector3 deltaVPerp = -(1 + Restitution) * vPerp;
        Vector3 deltaVPara = -Math.Min(1, Friction * ratio) * Mu * s;

        rb.velocity += deltaVPara + deltaVPerp;
        rb.angularVelocity += A * Radius * Vector3.Cross(deltaVPara, n);
    }

    private void PerformPlayerHit(Collision col)
    {
        Vector3 collisionPoint = col.rigidbody.ClosestPointOnBounds(rb.position); // col.GetContact(0).point;
        CancelUnityImpulse();
        col.gameObject.GetComponent<Resettable>().CancelUnityImpulse();

        Vector3 jBullet = -CustomPhysics.CalculateBulletImpulse(rb, col.rigidbody, collisionPoint);
        Vector3 jPsyonix = CustomPhysics.CalculatePsyonixImpulse(rb, col, pysionixImpulseCurve);

        
        CustomPhysics.ApplyImpulseAtPosition(rb, jBullet, collisionPoint);
        CustomPhysics.ApplyImpulseAtPosition(rb, jPsyonix, rb.position);

        CustomPhysics.ApplyImpulseAtPosition(col.rigidbody, -jBullet, collisionPoint);
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }


}



