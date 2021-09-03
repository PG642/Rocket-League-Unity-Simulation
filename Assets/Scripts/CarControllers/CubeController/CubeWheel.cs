﻿using System;
using System.Collections;
using System.Collections.Generic;
using CarControllers.CubeController;
using UnityEditor;
using UnityEngine;

public class CubeWheel : MonoBehaviour
{
  //  public float steerAngle;
  //  public float Fx;
    float Fy;

    private AnimationCurve _curve;
    private AnimationCurve _curve2;
    private AnimationCurve _steeringCurve;
    
    public bool wheelFL, wheelFR, wheelRL, wheelRR;
    
    public Transform wheelMesh;
    private float _meshRevolutionAngle;
    
    Rigidbody _rb;
    CubeController _c;
    CubeGroundControl _groundControl;
    InputManager _inputManager;

    float _wheelRadius, _wheelForwardVelocity, _wheelLateralVelocity;
    Vector3 _wheelVelocity, _lastWheelVelocity, _wheelAcceleration, _wheelContactPoint, _lateralForcePosition = Vector3.zero;
    
    private const float ForwardDragWheels = 5.25f;
    private const float ForwardDragRoof = 2.5f;

    private Vector3 _currentDriftForce = Vector3.zero;
    
    //[HideInInspector]
    public bool isDrawWheelVelocities, isDrawWheelDisc, isDrawForces;

    void Start()
    {
        var wheels = GetComponentInParent<CubeWheels>();
        _curve = wheels.Curve;
        _curve2 = wheels.Curve2;
        _steeringCurve = wheels.SteeringCurve;
        _rb = GetComponentInParent<Rigidbody>();
        _c = GetComponentInParent<CubeController>();
        _inputManager = GetComponentInParent<InputManager>();
        _groundControl= GetComponentInParent<CubeGroundControl>();
        _wheelRadius = transform.localScale.z / 2;
    }
    
    public void RotateWheels(float steerAngle)
    {
        if(wheelFL || wheelFR)
            transform.localRotation = Quaternion.Euler(Vector3.up * steerAngle);
        
        // Update mesh rotations of the wheel
        if (wheelMesh)
        {
            //wheelMesh.transform.position = transform.position;
            wheelMesh.transform.localRotation = transform.localRotation;
            _meshRevolutionAngle += (Time.deltaTime * transform.InverseTransformDirection(_wheelVelocity).z) /
                (2 * Mathf.PI * _wheelRadius) * 360;
            wheelMesh.transform.Rotate(Vector3.right, _meshRevolutionAngle * 1.3f);
            //transform.Rotate(new Vector3(0, 1, 0), steerAngle - transform.localEulerAngles.y);
        }
    }

    private void FixedUpdate()
    {
        UpdateWheelState();

        if(_c.isCanDrive)
            ApplyLateralForce();
        if(_c.carState != CubeController.CarStates.Air)
            SimulateDrag();
    }
    
    public void ApplyForwardForce(float force)
    {
        _rb.AddForce(force * transform.forward, ForceMode.Acceleration);
        
        // Kill velocity to 0 for small car velocities
        if (force == 0 && _c.forwardSpeedAbs < 0.1 && !_inputManager.isDrift)
            _rb.velocity -= Vector3.Dot(_rb.velocity, transform.forward) * transform.forward;
    }

    private void ApplyLateralForce()
    {
        if (Mathf.Abs(_wheelLateralVelocity) <= 0.001f) return;

        const float impulseMult = 5.0f;
        const float driftImpulseMult = 0.5f;

        var ratio = Mathf.Clamp01(Mathf.Abs(_wheelLateralVelocity) /
                                  (Mathf.Abs(_wheelLateralVelocity) + Mathf.Abs(_wheelForwardVelocity)));
        float slideFriction = (_inputManager.isDrift ? driftImpulseMult : impulseMult) * _curve.Evaluate(ratio);
        // var groundFriction = _curve2.Evaluate(RoboUtils.Scale(-1, 1, 0, 1, -_c.transform.up.y));
        var groundFriction = 1.0f;
        var friction = slideFriction * groundFriction;
        var constraint = -_wheelLateralVelocity;

        _lateralForcePosition = transform.position;
        // TODO: Behalten oder entsorgen? 
        //_c.cogLow.localPosition = Vector3.zero + (_inputManager.isDrift ? 0.0f : 0.0875f) * new Vector3(0.0f, 0.0f, 1.0f); 
        _lateralForcePosition.y = _c.cogLow.position.y;
        // TODO: Behalten oder entsorgen? 
        //_lateralForcePosition += (_inputManager.isDrift ? -0.0875f : 0.0f) * transform.forward;

        float steeringFactor;
        
        steeringFactor = 0.0f;
        if(_inputManager.isDrift)
        {
            if (wheelFL || wheelFR)
            {
                steeringFactor = Mathf.Sign(_inputManager.steerInput) * _steeringCurve.Evaluate(Mathf.Abs(_inputManager.steerInput));
            }
            else
            {
                steeringFactor = Mathf.Sign(_inputManager.steerInput) * 1.0f;
            }
        }
        steeringFactor = steeringFactor * Mathf.Sign(_inputManager.throttleInput);
        var impulse =  friction * constraint + steeringFactor;
        _rb.AddForceAtPosition(impulse * transform.right, _lateralForcePosition, ForceMode.Impulse);
    }

    private void SimulateDrag()
    {
        //Applies auto braking if no input, simulates air and ground drag
        if ( _c.forwardSpeedAbs < 0.1 || _inputManager.isDrift) return;
        
        var dragForce = (_c.isAllWheelsSurface ? ForwardDragWheels : ForwardDragRoof) / 4 * _c.forwardSpeedSign * 
                        (1 - Mathf.Abs(_inputManager.throttleInput));

        
        _rb.AddForce(-dragForce * transform.forward, ForceMode.Acceleration);
    }

    private void UpdateWheelState()
    {
        _wheelContactPoint = transform.position - transform.up * _wheelRadius;
        _wheelVelocity = _rb.GetPointVelocity(_wheelContactPoint);
        _wheelForwardVelocity = Vector3.Dot(_wheelVelocity, transform.forward);
        _wheelLateralVelocity = Vector3.Dot(_wheelVelocity, transform.right);

        _wheelAcceleration = (_wheelVelocity - _lastWheelVelocity) * Time.fixedTime;
        _lastWheelVelocity = _wheelVelocity;
    }

    #region DrawDebugGizmos

    private void OnDrawGizmos()
    {
        //_wheelRadius = transform.localScale.z / 2;

        // DrawWheelDisc();
        // DrawWheelContactPoint();
        //
        // if (isDrawWheelVelocities)
        //     DrawWheelVelocities();
        //
        // if(isDrawForces)
        //     DrawForces();
    }

    private void DrawWheelDisc()
    {
#if UNITY_EDITOR
        if (isDrawWheelDisc)
        {
            Handles.color = Color.black;
            if (_rb != null)
                Handles.color = _c.isCanDrive ? Color.green : Color.red;

            Handles.DrawWireArc(transform.position, transform.right, transform.up, 360, _wheelRadius);
        }
#endif
    }

    private void DrawWheelContactPoint()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position - transform.up * _wheelRadius, 0.02f);
    }

    private void DrawWheelVelocities()
    {
        if (_rb == null) return;
        if (_c.isCanDrive != true) return;   
        
        var offset = 0.05f * transform.up;
        RoboUtils.DrawRay(_wheelContactPoint + offset, _wheelVelocity * 0.1f, Color.black);
        RoboUtils.DrawRay(_wheelContactPoint + offset, (_wheelForwardVelocity * 0.1f) * transform.forward, Color.blue);
        RoboUtils.DrawRay(_wheelContactPoint + offset, (_wheelLateralVelocity * 0.1f) * transform.right, Color.red);
    }

    private void DrawForces()
    {
        if (_c.isCanDrive != true) return;   
        
        // Draw induced lateral friction Fy
        RoboUtils.DrawRay(_lateralForcePosition, 0.3f * -Fy * transform.right, Color.magenta);

        // Draw observed forces
        RoboUtils.DrawLocalRay(transform, transform.up, _wheelAcceleration.z, transform.forward, Color.gray);
    }
    
    #endregion
}