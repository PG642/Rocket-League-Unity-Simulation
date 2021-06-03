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

    float _rotationUpdateTime = 0.25f;
    float _lastRotationUpdate = 0f;
    float _rotationX = 0f;
    float _rotationY = 0f;

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
    }

    Vector3 AddRotation(Vector3 startPos)
    {
        float sensitivityX = 0.01f;
        float sensitivityY = 0.01f;
        float minimumX = -120f;
        float maximumX = 120f;
        float minimumY = -120F;
        float maximumY = 120F;


        Vector3 axis = Vector3.zero;
        if(Time.time - _lastRotationUpdate >= _rotationUpdateTime)
        {
            axis += new Vector3(_rotationY, _rotationX, 0);
            _rotationX = 0f;
            _rotationY = 0f;
            _lastRotationUpdate = Time.time;
        }
        else
        {
            _rotationX += Input.GetAxis("Camera X") * sensitivityX;
            _rotationX = Mathf.Clamp(_rotationX, minimumX, maximumX);
            _rotationY += Input.GetAxis("Camera Y") * sensitivityY;
            _rotationY = Mathf.Clamp(_rotationY, minimumY, maximumY);
        }


        return Quaternion.Euler(axis) * (startPos - _pivotPosition) + _pivotPosition;
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
        desiredPosition = AddRotation(desiredPosition);
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