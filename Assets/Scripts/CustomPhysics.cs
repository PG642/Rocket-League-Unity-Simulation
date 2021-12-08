using System;
using System.Linq;
using Consolation;
using UnityEngine;

public static class CustomPhysics
{

    public static float friction = 2f;
    public static Vector3 CalculatePsyonixImpulse(Rigidbody ball, Collision col, AnimationCurve pysionixImpulseCurve)
    {
        Vector3 n = ball.position - col.rigidbody.position;
        n.y *= 0.35f;
        Vector3 f = col.transform.forward;
        n = Vector3.Normalize(n - 0.35f * Vector3.Dot(n, f) * f);
        Vector3 J = ball.mass * col.relativeVelocity.magnitude *
                pysionixImpulseCurve.Evaluate(col.relativeVelocity.magnitude / 10f) * n;
        return J;
    }

    public static Vector3 CalculateBulletImpulse(Rigidbody self, Rigidbody other, Vector3 collisionPoint)
    {
        Rigidbody ballRigidBody = self;
        Rigidbody carRigidBody = other;
        Matrix4x4 Lc = CalculateMatrixL(carRigidBody.position, collisionPoint);
        Matrix4x4 Lb = CalculateMatrixL(ballRigidBody.position, collisionPoint);

        Matrix4x4 Ic = CalculateInertiaTensorMatrix(carRigidBody.inertiaTensor, carRigidBody.inertiaTensorRotation);
        Matrix4x4 Ib = CalculateInertiaTensorMatrix(ballRigidBody.inertiaTensor, ballRigidBody.inertiaTensorRotation);

        Matrix4x4 scaleIdentity = Matrix4x4.identity;
        float massScale = ((1 / carRigidBody.mass) + (1 / ballRigidBody.mass));
        scaleIdentity[0, 0] = massScale;
        scaleIdentity[1, 1] = massScale;
        scaleIdentity[2, 2] = massScale;
 
        Matrix4x4 M = Subtract( Subtract(scaleIdentity, (Lc* Ic.inverse * Lc)) , Lb* Ib.inverse * Lb ).inverse;
        Vector3 carV = carRigidBody.velocity - (Vector3)(Lc * carRigidBody.angularVelocity);
        Vector3 ballV =  ballRigidBody.velocity - (Vector3)(Lb * ballRigidBody.angularVelocity);
        Vector3 deltaV = carV - ballV;
        Vector3 J = Subtract(Matrix4x4.zero, M) * deltaV;

        //n ist normale des aufprallpunkts. Wir aktuell für kollision mit kugel berechnet, bei kollision mit auto muss verallgemeinerte formel her
        Vector3 n = (collisionPoint - ballRigidBody.position) / (collisionPoint - ballRigidBody.position).magnitude;

        Vector3 Jperp = Vector3.Dot(J, n) * n;
        Vector3 Jpara = J - Jperp;
        J = Jperp + Math.Min(1, friction * Jperp.magnitude / Jpara.magnitude) * Jpara;

        return J;
    }

    public static Vector3 CalculateVelocityAfterImpulse(Rigidbody rb, Vector3 J)
    {
        return rb.velocity + J / rb.mass;
    }

    public static Vector3 CalculateAngularVelocityAfterImpulse(Rigidbody rb, Vector3 J, Vector3 collisionPoint)
    {
        Vector3 deltaOmega = CalculateInertiaTensorMatrix(rb.inertiaTensor, rb.inertiaTensorRotation).inverse * CalculateMatrixL(rb.position, collisionPoint) * J;
        return rb.angularVelocity - deltaOmega;
    }

    public static void ApplyImpulseAtPosition(Rigidbody rb, Vector3 J, Vector3 position)
    {
        rb.velocity = CalculateVelocityAfterImpulse(rb, J);
        rb.angularVelocity = CalculateAngularVelocityAfterImpulse(rb, J, position);
    }

    private static Matrix4x4 CalculateMatrixL(Vector3 rbPosition, Vector3 collisionPoint)
    {
        Vector3 dist = collisionPoint - rbPosition;
        return new Matrix4x4(
            new Vector4(      0, -dist.z,  dist.y, 0),
            new Vector4( dist.z,       0, -dist.x, 0),
            new Vector4(-dist.y,  dist.x,       0, 0),
            new Vector4(      0,       0,       0, 1));
    }


    private static Matrix4x4 CalculateInertiaTensorMatrix(Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
    {
        Matrix4x4 R = Matrix4x4.Rotate(inertiaTensorRotation); //rotation matrix created
        Matrix4x4 S = Matrix4x4.Scale(inertiaTensor); // diagonal matrix created
        return R * S * R.inverse; // R is orthogonal, so R.transpose == R.inverse
    }

    private static Matrix4x4 Subtract(Matrix4x4 first, Matrix4x4 second)
    {
        Matrix4x4 result = Matrix4x4.identity;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                result[i, j] = first[i, j] - second[i, j];
            }
        }
        return result;
    }


}