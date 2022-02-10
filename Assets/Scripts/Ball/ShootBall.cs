using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    public Transform ShootAt;
    public Vector2 speed = new Vector2(50, 100);
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }


    public void ShootTarget(float velocity = 0.0f)
    {
        Vector3 dir = ShootAt.position - transform.position;
        if (velocity == 0.0f)
        {
            velocity = UnityEngine.Random.Range(speed.x, speed.y);
        }
        
        _rb.AddForce( dir.normalized  * velocity, ForceMode.VelocityChange);
    }
}
