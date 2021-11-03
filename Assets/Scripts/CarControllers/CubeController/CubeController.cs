using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{
    [Header("Car State")] public bool isAllWheelsSurface = false;
    public bool isCanDrive;
    public float forwardSpeed = 0, forwardSpeedSign = 0, forwardSpeedAbs = 0;
    public int numWheelsSurface;
    public bool isBodySurface;
    public CarStates carState;

    [Header("Other")] public Transform cogLow;
    public GameObject sceneViewFocusObject;

    public const float MaxSpeedBoost = 23.00f;

    Rigidbody _rb;
    GUIStyle _style;
    GroundTrigger[] _sphereColliders;

    public enum CarStates
    {
        AllWheelsGround,
        Air,
        AllWheelsSurface,
        SomeWheelsSurface,
        BodySideGround,
        BodyGroundDead
    }

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _rb.centerOfMass = cogLow.localPosition;
        _rb.maxAngularVelocity = 5.5f;

        _sphereColliders = GetComponentsInChildren<GroundTrigger>();

        // GUI stuff
        _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        _style.fontSize = 25;
        _style.fontStyle = FontStyle.Bold;

        // Lock scene view camera to the car
// #if UNITY_EDITOR
//         Selection.activeGameObject = sceneViewFocusObject;
//         SceneView.lastActiveSceneView.FrameSelected(true);
// #endif
    }

    void FixedUpdate()
    {
        SetCarState();
        UpdateCarVariables();
    }
    

    private void LateUpdate()
    {
        if (_rb.velocity.magnitude > MaxSpeedBoost)
        {
            _rb.velocity = _rb.velocity.normalized * MaxSpeedBoost;
        }
    }

    public void ResetCar(Vector3 position, Quaternion rotation)
    {
        _rb.position = position;
        _rb.rotation = rotation;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        GetComponent<CubeJumping>().Reset();
        GetComponent<CubeBoosting>()._boostAmount = 32f;
    }

    private void UpdateCarVariables()
    {
        
        
        forwardSpeed = Vector3.Dot(_rb.velocity, transform.forward);

        var vectorForwardSpeed = forwardSpeed * transform.forward; 
        
        forwardSpeed = Math.Abs(forwardSpeed) > 0.02f ?  (float) System.Math.Round(forwardSpeed, 2): 0.0f;

        if (forwardSpeed == 0)
        {
            _rb.velocity -= vectorForwardSpeed;
        }
        
        

        Debug.Log($"{forwardSpeed} + Time {Time.frameCount}");

        forwardSpeedAbs = Mathf.Abs(forwardSpeed);
        forwardSpeedSign = Mathf.Sign(forwardSpeed);
    }

    void SetCarState()
    {
        numWheelsSurface
            = _sphereColliders.Count(c => c.isTouchingSurface);

        isAllWheelsSurface = numWheelsSurface >= 3;

        // All wheels are touching the ground
        if (isAllWheelsSurface)
            carState = CarStates.AllWheelsSurface;

        // Some wheels are touching the ground, but not the body
        if (!isAllWheelsSurface && !isBodySurface)
            carState = CarStates.SomeWheelsSurface;

        // We are lying on our side
        if (isBodySurface && !isAllWheelsSurface)
            carState = CarStates.BodySideGround;

        // All wheels on the ground
        if (isAllWheelsSurface && Vector3.Dot(Vector3.up, transform.up) > 0.95f)
            carState = CarStates.AllWheelsGround;

        // He is dead Jimmy!
        if (isBodySurface && Vector3.Dot(Vector3.up, transform.up) < -0.95f)
            carState = CarStates.BodyGroundDead;

        // In the air
        if (!isBodySurface && numWheelsSurface == 0)
            carState = CarStates.Air;

        isCanDrive = carState == CarStates.AllWheelsSurface || carState == CarStates.AllWheelsGround;
    }

    void DownForce()
    {
        if (carState == CarStates.AllWheelsSurface || carState == CarStates.AllWheelsGround)
            _rb.AddForce(-transform.up * 5, ForceMode.Acceleration);
    }

    # region GUI

    void OnGUI()
    {
        GUI.Label(new Rect(10.0f, 40.0f, 150, 130), $"{forwardSpeed:F2} m/s {forwardSpeed * 100:F0} uu/s", _style);
        //GUI.Label(new Rect(30.0f, 40.0f, 150, 130), string.Format("turnRadius: {0:F2} m curvature: {1:F4}", turnRadius, curvature), style);
        //GUI.Label(new Rect(30.0f, 60.0f, 150, 130), $"car state: {carState.ToString()}", _style);
    }

    private void OnDrawGizmos()
    {
        // Draw CG
        if (_rb == null) return;
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_rb.transform.TransformPoint(_rb.centerOfMass), 0.03f);
    }

    #endregion
}