using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarCollision : Resettable
{
    private MatchController.MatchController _matchController;
    private float forwardSpeed;
    private SuspensionCollider[] _suspensionColliders;
    public Vector3 surfaceNormal;
    private float _bumpingForce = 4f;
    private float _bumpingUpForce = 8f;
    public AnimationCurve pysionixImpulseCurve = new AnimationCurve();


    private float _bumpingTorque = 8f;
    private float _lastTimeSuperSonic = 0f;
    private float _lastTimeCollided = 0f;
    private PreviousCarState previousState;



    void Start()
    {
        base.Start();
        previousState = GetComponent<PreviousCarState>();
        _matchController = transform.GetComponentInParent<MatchController.MatchController>();
        _suspensionColliders = GetComponentsInChildren<SuspensionCollider>();
    }


    public override void FixedUpdate()
    {
        base.FixedUpdate();
        forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        _lastTimeSuperSonic = forwardSpeed >= 22f ? Time.time : _lastTimeSuperSonic;

    }

    private void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Ground"))
        {
            surfaceNormal = collisionInfo.contacts[0].normal;
        }
        if (collisionInfo.gameObject.CompareTag("ControllableCar"))
        {
            DoCarCarInteraction(collisionInfo);
        }
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Ground"))
        {
            surfaceNormal = collisionInfo.contacts[0].normal;
        }
    }

    private void OnCollisionExit(Collision collisionInfo)
    {
    }


    private void DoCarCarInteraction(Collision collisionInfo)
    {
        if (Time.time - _lastTimeCollided < 0.001f)
        {
            RestoreCarPhysics();
            return;
        }
        _lastTimeCollided = Time.time;
        TeamController teamController = GetComponentInParent<TeamController>();
        if (teamController.GetTeamOfCar(collisionInfo.gameObject) == teamController.GetTeamOfCar(gameObject))
            return;

        Vector3 directionToOtherCogLow = collisionInfo.transform.Find("CubeController").Find("cogLow").position -
                                         transform.Find("CubeController").Find("cogLow").position;

        Vector3 horizontalDirection = Vector3.ProjectOnPlane(directionToOtherCogLow, transform.up);
        //Debug.DrawRay(transform.position, horizontalDirection, Color.red, 3f);
        Vector3 verticalDirection = Vector3.ProjectOnPlane(directionToOtherCogLow, transform.right);
        //Debug.DrawRay(transform.position, verticalDirection.normalized * 3f, Color.blue, 3f);

        float horizontalAngle = Vector3.Angle(horizontalDirection, transform.forward);
        float verticalAngle = Vector3.Angle(verticalDirection.normalized * 3f, transform.forward);

        //Debug.Log(teamController.GetTeamOfCar(gameObject) + " horizontal :" + horizontalAngle);
        //Debug.Log(teamController.GetTeamOfCar(gameObject) + " vertical :" + verticalAngle);


        if (horizontalAngle <= 45 && verticalAngle <= 37 && forwardSpeed >= 21f && Time.time - _lastTimeSuperSonic < 1f)
        {
            //Debug.Log("DEMO!");
            _matchController.HandleDemolition(collisionInfo.gameObject);
            RestoreCarPhysics();
        }
        else if (horizontalAngle <= 70 && verticalAngle <= 70)
        {
            //Debug.Log("BUMPING!");
            GameObject bumpedCar = collisionInfo.gameObject;
            RestoreBumperPhysics(bumpedCar, directionToOtherCogLow.normalized, collisionInfo);

        }
        else
        {
            //Debug.Log("NOTHING!");
        }
    }

    void RestoreCarPhysics()
    {
        rb.velocity = previousState.velocity;
        rb.angularVelocity = previousState.angularVelocity;

        transform.eulerAngles = previousState.eulerAngles + Time.deltaTime * previousState.angularVelocity;
        transform.position = previousState.position + Time.deltaTime * previousState.velocity;
    }

    void RestoreBumperPhysics(GameObject bumpedCar, Vector3 directionToOtherCogLow, Collision col)
    {

        CancelUnityImpulse();
        Rigidbody bumpedRb = bumpedCar.GetComponent<Rigidbody>();
        bumpedCar.GetComponent<CarCollision>().CancelUnityImpulse();
        Vector3 averageContactPoint = Vector3.zero;
        Vector3 averageContactNormal = Vector3.zero;
        foreach (ContactPoint cp in col.contacts)
        {
            averageContactPoint += cp.point;
            averageContactNormal += cp.normal;
        }
        averageContactPoint /= col.contacts.Length;
        averageContactNormal /= col.contacts.Length;
        Debug.DrawRay(averageContactPoint, averageContactNormal * 3f, Color.red, 3f);
        Vector3 jBullet = CustomPhysics.CalculateBulletImpulse(rb, bumpedRb, averageContactPoint);
        Vector3 jPsyonix = -CustomPhysics.CalculatePsyonixImpulse(rb, col, pysionixImpulseCurve);

        //Debug.Log("Bullet" + jBullet.ToString());
        //Debug.Log("Psyonix" + jPsyonix.ToString());
        CustomPhysics.ApplyImpulseAtPosition(bumpedRb, Vector3.Scale(jBullet, new Vector3(_bumpingForce, _bumpingForce, _bumpingForce)), averageContactPoint); // col.GetContact(0).point-4.0f*Vector3.up);
        CustomPhysics.ApplyImpulseAtPosition(bumpedRb, Vector3.Scale(jPsyonix, new Vector3(1f, 1f, 1f)), bumpedRb.position); // col.GetContact(0).point-4.0f*Vector3.up);
        CustomPhysics.ApplyImpulseAtPosition(rb, -jBullet, averageContactPoint);

        return;
        // PreviousCarState bumpedCarPrevious = bumpedCar.GetComponent<PreviousCarState>();
        // float bumpervel = Mathf.Abs(Vector3.Dot(previousState.velocity.normalized, directionToOtherCogLow) * previousState.velocity.magnitude);
        // float bumpedvel = Mathf.Abs(Vector3.Dot(bumpedCarPrevious.velocity.normalized, -directionToOtherCogLow) * bumpedCarPrevious.velocity.magnitude);
        // bumpervel = previousState.velocity.magnitude < 1e-4 ? 0 : bumpervel;
        // bumpedvel = bumpedCarPrevious.velocity.magnitude < 1e-4 ? 0 : bumpedvel;
        // directionToOtherCogLow.y = Mathf.Abs(directionToOtherCogLow.y) < 1e-1 ? 0 .1f: directionToOtherCogLow.y;
        // rb.velocity = previousState.velocity - directionToOtherCogLow * ((rb.mass * bumpervel + bumpedRb.mass * bumpedvel * (2f * bumpedvel - bumpervel))) / (rb.mass + bumpedRb.mass);
        // bumpedRb.velocity = bumpedCarPrevious.velocity + _bumpingForce * directionToOtherCogLow * ((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel))) / (bumpedRb.mass + rb.mass);
        //Debug.Log((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel)) / (bumpedRb.mass + rb.mass));
        //Debug.Log(directionToOtherCogLow.ToString());
        //Debug.Log(directionToOtherCogLow * ((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel))) / (bumpedRb.mass + rb.mass));

        //transform.position = previousState.position + Time.deltaTime * rb.velocity;
        //bumpedCar.transform.position = bumpedCarPrevious.position + Time.deltaTime * bumpedRb.velocity;
    }

}
