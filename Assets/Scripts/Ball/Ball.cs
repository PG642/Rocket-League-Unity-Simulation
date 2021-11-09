using System;
using System.Diagnostics;
using Consolation;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField] [Range(10, 80)] float randomSpeed = 40;
    [SerializeField] float initialForce;
    [SerializeField] float hitMultiplier;
    public float _maxAngluarVelocity = 6.0f;
    public float _maxVelocity = 60.0f;
    public AnimationCurve pysionixImpulseCurve = new AnimationCurve();
    public bool isTouchedGround = false;
    private float _minVelocity = 0.4f;
    private float _minAngularVelocity = 1.047f;
    private float _lastStoppedTime;


    private Vector3 _position;
    private Vector3 _velocity;
    private Quaternion _rotation;
    private Vector3 _angularVelocity;
    Rigidbody _rb;
    Transform _transform;
    private float _timeWindowToStop = 2.0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        

        _transform = this.transform;
        isTouchedGround = false;
        _rb.maxAngularVelocity = _maxAngluarVelocity;
        _rb.maxDepenetrationVelocity = _maxVelocity;
    }

    void Update()
    {
        //TODO: move inputs to the InputController
        if (Input.GetKeyDown(KeyCode.T))
            ShootInRandomDirection(randomSpeed);

        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();

        if (Input.GetButtonDown("Select"))
            ResetShot(new Vector3(7.76f, 2.98f, 0f));
        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxVelocity);
    }

    private void FixedUpdate()
    {
        Clone( _rb);
    }

    private void Clone(Rigidbody rb)
    {
        
        _position = rb.position;

        _position = new Vector3
        {
            x = _position.x,
            y = _position.y,
            z = _position.z,
        };
        _rotation = new Quaternion((_rotation = rb.rotation).x, _rotation.y, _rotation.z, _rotation.w);
        _angularVelocity = new Vector3(
            _angularVelocity.x,
            _angularVelocity.y,
            _angularVelocity.z);
        _velocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
    }

    private void LateUpdate()
    {
        if (_rb.velocity.magnitude > _maxVelocity)
        {
            _rb.velocity = _rb.velocity.normalized * _maxVelocity;
        }

        if (_rb.angularVelocity.magnitude > _maxAngluarVelocity)
        {
            _rb.angularVelocity = _rb.angularVelocity.normalized * _maxVelocity;
        }

        StopBallIfTooSlow();
    }

    private void ResetShot(Vector3 pos)
    {
        _transform.position = pos;
        _rb.velocity = new Vector3(30, 10, 0);
        _rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ResetBall")]
    public void ResetBall()
    {
        var desired = new Vector3(0, 12.23f, 0f);
        _transform.SetPositionAndRotation(desired, Quaternion.identity);
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    [ContextMenu("ShootInRandomDirection")]
    private void ShootInRandomDirection(float speed)
    {
        float speedRange = Random.Range(speed - 10, speed + 10);
        var randomDirection = Random.insideUnitCircle.normalized;
        var direction = new Vector3(randomDirection.x, Random.Range(-0.5f, 0.5f), randomDirection.y).normalized;
        _rb.velocity = direction * speedRange;
    }

    private void StopBallIfTooSlow()
    {
        if (_rb.velocity.magnitude <= _minVelocity && _rb.angularVelocity.magnitude <= _minAngularVelocity)
        {
            if (_lastStoppedTime == 0.0f)
            {
                _lastStoppedTime = Time.time;
            }

            if (_lastStoppedTime < Time.time - _timeWindowToStop && _lastStoppedTime > 0.0f)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            _lastStoppedTime = 0.0f;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Ground"))
        {
            SetBallOneFrameBack();
            Debug.Log($" Neu : {_rb.position.x} Alt: {_position.x}");
            Debug.Log($"Hit : {col.GetContact(0).normal} {(_rb.position-col.GetContact(0).point).normalized}");
            float impulseMagnitude = initialForce + col.impulse.magnitude * hitMultiplier;
            //Vector3 dir = transform.position - col.contacts[0].point;
            var dir = _rb.position - col.contacts[0].point;
            Debug.Log($"{dir.x/dir.y}");
            _rb.AddForce(CalculatePsyonixImpulse(col), ForceMode.Impulse);
            _rb.AddForce(dir.normalized * impulseMagnitude, ForceMode.Impulse);
            Debug.Log($" Force : {dir.normalized * impulseMagnitude}");
        }

        if (col.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = true;
        }

        //if (col.gameObject.tag == "Ground")
        //    if (rb.velocity.y > 3)
        //    {
        //    //rb.AddForce(Vector3.up * -downForce);
        //        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - SlowVelocityGround, rb.velocity.z);
        //    }
    }

    private void SetBallOneFrameBack()
    {
        _rb.position = _position + _velocity * Time.deltaTime;
        _rb.velocity = _velocity;
        _rb.angularVelocity = _angularVelocity;
        _rb.rotation = _rotation;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }

    Vector3 CalculatePsyonixImpulse(Collision col)
    {
        var n = _rb.position - col.rigidbody.position;
        n.y *= 0.35f;
        
        Debug.Log($"y : {n.y/n.x}");
        var f = col.transform.forward;
        var dot = Vector3.Dot(n, f);
        n = Vector3.Normalize(n - 0.35f * dot * f);
        var impulse = _rb.mass * Math.Abs(col.relativeVelocity.magnitude) * scaling(col.relativeVelocity.magnitude) *
                      n; // TODO scaling
        //Debug.Log(impulse);
        //Debug.Log(col.relativeVelocity.magnitude);
        return impulse;
    }

    float scaling(float magninute)
    {
        var test = pysionixImpulseCurve.Evaluate(magninute/10f);
        
        Debug.Log($"{Time.time}: Curve {test} : Value {magninute}");
        return test;
    }
}