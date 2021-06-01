using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootBall : MonoBehaviour
{
    public Transform Target;
    public Vector2 speed = new Vector2(50, 100);
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Vector3 targetPos = Target.position;
        ShootTarget();
    }

    public void ShootTarget()
    {
        Vector3 dir = Target.position - transform.position;
        _rb.velocity = dir.normalized * (Random.Range(speed.x, speed.y));
    }
}
