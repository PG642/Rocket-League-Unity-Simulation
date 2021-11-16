using System;
using UnityEngine;

public static class CustomPhysics
{
    public static Vector3 CalculatePsyonixImpulse(Rigidbody ball, Collision col, AnimationCurve pysionixImpulseCurve)
    {
        var n = ball.position - col.rigidbody.position;
        n.y *= 0.35f;
        var f = col.transform.forward;
        n = Vector3.Normalize(n - 0.35f * Vector3.Dot(n, f) * f);
        var J = ball.mass * Math.Abs(col.relativeVelocity.magnitude) * pysionixImpulseCurve.Evaluate(col.impulse.magnitude/10f) * n;
        return J;
    }

    public static Vector3 CalculateBulletImpulse(Collision col)
    {
        
        return Vector3.zero;
    }
}