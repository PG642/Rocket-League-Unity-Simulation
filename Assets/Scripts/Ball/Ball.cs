using System;
using System.Diagnostics;
using Consolation;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField] [Range(10,80)] float randomSpeed = 40;
    [SerializeField] float initialForce ;
    [SerializeField] float hitMultiplier;
    public float _maxAngluarVelocity = 6.0f;
    public  float _maxVelocity = 60.0f;
    public AnimationCurve pysionixImpulseCurve = new AnimationCurve();
    public bool isTouchedGround = false;
    
    Rigidbody _rb;
    Transform _transform;

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

    private void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.CompareTag("Player"))
        {
            
            float force = initialForce + col.relativeVelocity.magnitude * hitMultiplier;
            //Vector3 dir = transform.position - col.contacts[0].point;
            var dir = _rb.position - col.transform.position;
            _rb.AddForce(CalculatePsyonixImpulse(col), ForceMode.Impulse);
            _rb.AddForce(dir.normalized * force);
            Debug.Log($" Force : {dir.normalized * force}");
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

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isTouchedGround = false;
        }
    }

    Vector3 CalculatePsyonixImpulse(Collision col)
    {
        var n = _rb.position - col.transform.position;
        n.y *= 0.35f;
        var f = col.transform.forward;
        var dot = Vector3.Dot(n, f);
        n = Vector3.Normalize(n - 0.35f * dot * f);
        var impulse = _rb.mass * Math.Abs(col.relativeVelocity.magnitude) * scaling(col.relativeVelocity.magnitude) * n; // TODO scaling
        //Debug.Log(impulse);
        //Debug.Log(col.relativeVelocity.magnitude);
        return impulse ;
    }

    float scaling(float magninute)
    {
        var test = pysionixImpulseCurve.Evaluate(magninute);
        
        Debug.Log($"{Time.time}: Curve {test} : Value {magninute}");
        return test ;
    }
}
