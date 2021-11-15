using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarCollision : MonoBehaviour
{
    private MatchController.MatchController _matchController;
    private float forwardSpeed;
    private Rigidbody rb;
    private SuspensionCollider[] _suspensionColliders;
    public Vector3 surfaceNormal;
    private float _bumpingForce = 1200.0f;
    private float _bumpingUpForce = 100.0f;
    private float _bumpingTorque = 10f;
    private float _lastTimeSuperSonic = 0f;
    private float _lastTimeCollided = 0f;
    private PreviousCarState previousState;

    

    void Start()
    {
        previousState = GetComponent<PreviousCarState>();
        _matchController = transform.GetComponentInParent<MatchController.MatchController>();
        rb = GetComponent<Rigidbody>();
        _suspensionColliders = GetComponentsInChildren<SuspensionCollider>();
    }


    private void FixedUpdate()
    {
        forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        _lastTimeSuperSonic = forwardSpeed >= 22f ? Time.time : _lastTimeSuperSonic;
    }
    
    private void OnCollisionEnter(Collision collisionInfo)
    {
        DoCarCarInteraction(collisionInfo);
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
        if (Time.time - _lastTimeCollided < 0.05f)
        {
            //RestoreCarPhysics();
            return;
        }
        _lastTimeCollided = Time.time;
        if (collisionInfo.gameObject.CompareTag("Ground"))
        {
            surfaceNormal = collisionInfo.contacts[0].normal;
        }

        if (!collisionInfo.gameObject.CompareTag("ControllableCar"))
            return;
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


        if (horizontalAngle <= 45 && verticalAngle <= 37 && forwardSpeed >= 21f && Time.time-_lastTimeSuperSonic < 1f)
        {
            Debug.Log("DEMO!");
            _matchController.HandleDemolition(collisionInfo.gameObject);
            RestoreCarPhysics();
        }
        else if (horizontalAngle <= 70 && verticalAngle <= 70)
        {
            Debug.Log("BUMPING!");
            GameObject bumpedCar = collisionInfo.gameObject;
            RestoreBumperPhysics(bumpedCar, directionToOtherCogLow);

        }
    }

    void RestoreCarPhysics()
    {
        rb.velocity = previousState.velocity;
        rb.angularVelocity = previousState.angularVelocity;
     
        transform.eulerAngles = previousState.eulerAngles + Time.deltaTime * previousState.angularVelocity;
        transform.position = previousState.position + Time.deltaTime * previousState.velocity;
    }

    void RestoreBumperPhysics(GameObject bumpedCar, Vector3 directionToOtherCogLow)
    {
        Rigidbody bumpedRb = bumpedCar.GetComponent<Rigidbody>();
        PreviousCarState bumpedCarPrevious = bumpedCar.GetComponent<PreviousCarState>();
        float bumpervel = Mathf.Abs(Vector3.Dot(previousState.velocity.normalized, directionToOtherCogLow) * previousState.velocity.magnitude);
        float bumpedvel = Mathf.Abs(Vector3.Dot(bumpedCarPrevious.velocity.normalized, -directionToOtherCogLow) * bumpedCarPrevious.velocity.magnitude);
        bumpervel = previousState.velocity.magnitude < 1e-4 ? 0 : bumpervel;
        bumpedvel = bumpedCarPrevious.velocity.magnitude < 1e-4 ? 0 : bumpedvel;
        rb.velocity = previousState.velocity;
        bumpedRb.velocity = bumpedCarPrevious.velocity;
        directionToOtherCogLow.y = Mathf.Abs(directionToOtherCogLow.y) < 1e-1 ? 0.1f: directionToOtherCogLow.y;
        rb.AddForce(-directionToOtherCogLow * ((rb.mass * bumpervel + bumpedRb.mass * bumpedvel * (2f * bumpedvel - bumpervel))) / (rb.mass + bumpedRb.mass), ForceMode.Impulse);
        bumpedRb.AddForce( directionToOtherCogLow * ((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel))) / (bumpedRb.mass + rb.mass), ForceMode.Impulse);
        Debug.Log((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel)) / (bumpedRb.mass + rb.mass));
        Debug.Log(directionToOtherCogLow.x);
        Debug.Log(directionToOtherCogLow.y);
        Debug.Log(directionToOtherCogLow.z);
        Debug.Log(directionToOtherCogLow * ((bumpedRb.mass * bumpedvel + rb.mass * bumpervel * (2f * bumpervel - bumpedvel))) / (bumpedRb.mass + rb.mass));

        transform.position = previousState.position + Time.deltaTime * rb.velocity;
        bumpedCar.transform.position = bumpedCarPrevious.position + Time.deltaTime * bumpedRb.velocity;
    }



}
