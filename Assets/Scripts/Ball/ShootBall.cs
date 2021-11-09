using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootBall : MonoBehaviour
{
    public Transform ShootAt;
    public Vector2 speed = new Vector2(50, 100);
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        ShootTarget();
    }

    public void ShootTarget()
    {
        Vector3 dir = ShootAt.position - transform.position;
        _rb.AddForce( dir.normalized  * Random.Range(speed.x, speed.y), ForceMode.VelocityChange);
    }
}
