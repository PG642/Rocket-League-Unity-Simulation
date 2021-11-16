using System;
using System.Linq;
using Consolation;
using UnityEngine;

public static class CustomPhysics
{
    public static Vector3 CalculatePsyonixImpulse(Rigidbody ball, Collision col, AnimationCurve pysionixImpulseCurve)
    {
        var n = ball.position - col.rigidbody.position;
        n.y *= 0.35f;
        var f = col.transform.forward;
        n = Vector3.Normalize(n - 0.35f * Vector3.Dot(n, f) * f);
        var J = ball.mass * Math.Abs(col.relativeVelocity.magnitude) *
                pysionixImpulseCurve.Evaluate(col.impulse.magnitude / 10f) * n;
        return J;
    }

    public static Vector3 CalculateBulletImpulse(Rigidbody self, Collision col, float friction)
    {
        
        var collisionPoint = col.contacts.First().point;
        var carRigidBody = col.rigidbody;
        var ballRigidBody = self;
        Matrix4x4 Lc = CalculateMatrixL(carRigidBody.position, collisionPoint);
        Matrix4x4 Lb = CalculateMatrixL(ballRigidBody.position, collisionPoint);

        Matrix4x4 Ic = CalculateInertiaTensorMatrix(carRigidBody.inertiaTensor, carRigidBody.inertiaTensorRotation);
        Matrix4x4 Ib = CalculateInertiaTensorMatrix(ballRigidBody.inertiaTensor, ballRigidBody.inertiaTensorRotation);
        
        Matrix4x4 scaleIdentity = Matrix4x4.identity;
        var massScale = (1 / carRigidBody.mass + 1 / ballRigidBody.mass);
        scaleIdentity[0, 0] = massScale;
        scaleIdentity[1, 1] = massScale;
        scaleIdentity[2, 2] = massScale;
 
        var M = Subtract( Subtract(scaleIdentity, (Lc* Ic.inverse * Lc)) , Lb* Ib.inverse * Lb ).inverse ;

        var carV = CalculateContactVelocity(carRigidBody, collisionPoint);

        var ballV = CalculateContactVelocity(ballRigidBody, collisionPoint);
        var deltaV = carV - ballV;
        Vector3 J = Subtract(Matrix4x4.zero, M) * deltaV;
        var n = (collisionPoint - carRigidBody.position).normalized;
        var Jperp = Vector3.Dot(J, n) * n;
        var Jpara = J - Jperp;
        J = Jperp + Math.Min(1, friction * Jperp.magnitude / Jpara.magnitude) * Jpara;
        
        return J;
    }

    private static Matrix4x4 CalculateMatrixL(Vector3 rbPosition, Vector3 collisionPoint)
    {
        Vector3 dist = collisionPoint - rbPosition;
        return new Matrix4x4(
            new Vector4(0, -dist.z, dist.y, 0),
            new Vector4(dist.z, 0, -dist.x, 0),
            new Vector4(-dist.y, dist.x, 0, 0),
            new Vector4(0, 0, 0, 1));
    }

    private static Vector3 CalculateContactVelocity(Rigidbody rb, Vector3 collisionPoint)
    {
        return rb.velocity + Vector3.Cross(rb.angularVelocity,
            collisionPoint - rb.position);
    }

    private static Matrix4x4 CalculateInertiaTensorMatrix(Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
    {
        Matrix4x4 R = Matrix4x4.Rotate(inertiaTensorRotation); //rotation matrix created
        Matrix4x4 S = Matrix4x4.Scale(inertiaTensor); // diagonal matrix created
        return R * S * R.transpose; // R is orthogonal, so R.transpose == R.inverse
    }

    private static Matrix4x4 Subtract(Matrix4x4 first, Matrix4x4 second)
    {
        Matrix4x4 result = Matrix4x4.zero;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                result[i,j] = first[i,j] - second[i,j];
            }
        }

        return result;
    }

}