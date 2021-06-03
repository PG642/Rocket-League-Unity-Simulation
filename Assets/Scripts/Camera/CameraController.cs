using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float distanceToBall = 20;

    [Header("Ballcam on settings")]
    public float cameraDist = 9f;
    public float cameraHeight = 3f;
    public float cameraAngle = 2.3f;
    public float stiffnessPosition = 50;
    public float stiffnessAngle = 30;


    Transform _ball, _car;
    Vector3 _pivotPosition;

    bool _isBallCam = false;
    void Start()
    {
        _ball = transform.parent.Find("Ball");
        _car = transform.parent.Find("ControllableCar");
        _pivotPosition = _car.position + Vector3.up * cameraHeight;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("X"))
            _isBallCam = !_isBallCam;
    }

    private void FixedUpdate()
    {
        UpdatePivotElement(stiffnessPosition);
        UpdateCamDirection(stiffnessAngle);
        UpdateCamPositon(stiffnessPosition);
        AddRotation(stiffnessAngle);
    }

    void AddRotation(float stiffnessAngle)
    {
        Vector3 axis = new Vector3(0, Input.GetAxis("Camera X"), Input.GetAxis("Camera Y"));
        transform.RotateAround(_pivotPosition, axis, stiffnessAngle * Time.deltaTime);
    }
    void UpdatePivotElement(float stiffnessPos)
    {
        Vector3 desiredPosition;
        CubeController.CarStates carState = _car.GetComponentInChildren<CubeController>().carState;
        bool grounded = carState == CubeController.CarStates.AllWheelsGround || carState == CubeController.CarStates.AllWheelsSurface;
        if (grounded)
        {
            desiredPosition = _car.position + _car.up * cameraHeight;
        }
        else
        {
            desiredPosition = _car.position + Vector3.up * cameraHeight;
        }
        _pivotPosition = Vector3.Lerp(_pivotPosition, desiredPosition, stiffnessPos * Time.deltaTime);
    }

    void UpdateCamPositon(float stiffnessPos)
    {
        Vector3 desiredPosition;
        CubeController.CarStates carState = _car.GetComponentInChildren<CubeController>().carState;
        bool grounded = carState == CubeController.CarStates.AllWheelsGround || carState == CubeController.CarStates.AllWheelsSurface;
        if (_isBallCam)
        {
            desiredPosition = _pivotPosition + (_car.position - _ball.position).normalized * cameraDist;
        }
        else
        {
            if (grounded)
            {
                desiredPosition = _pivotPosition - _car.forward * cameraDist;
            }
            else
            {
                desiredPosition = _pivotPosition - _car.GetComponent<Rigidbody>().velocity.normalized * cameraDist;
            }
        }
        if (grounded)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, stiffnessPos * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.2f * stiffnessPos * Time.deltaTime);
        }

    }

    void UpdateCamDirection(float stiffnessAngle)
    {
        Vector3 angleOffset = new Vector3(0, cameraAngle, 0);
        Vector3 desiredAngle = _pivotPosition - transform.position + angleOffset;
        Quaternion rot = Quaternion.LookRotation(desiredAngle, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, stiffnessAngle * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, Ball.position);
    }

    RaycastHit Raycast(Vector3 origin, Vector3 direction, float maxDist)
    {
        Physics.Raycast(origin, direction, out RaycastHit hit, maxDist);
        return hit;
    }
    bool isRaycast(Vector3 origin, Vector3 direction, float maxDist)
    {
        return Physics.Raycast(origin, direction, maxDist);
    }
}