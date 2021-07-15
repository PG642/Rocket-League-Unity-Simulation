using System;
using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeJumping : MonoBehaviour
{
    float _timerJumpButtonHeld = 0;
    float _timerSecondJump = 0;

    bool _isFirstJumpPress = false;
    public bool IsFirstJump = false;
    public bool IsSecondJump = false;
    bool _isSecondJumpUsed = false;
    bool _lowerSecondJumpTimer = false;
    bool _lastFrameNoButton = true;

    public bool IsDodge = false;
    public float TimerDodge = 0f;
    const float _dodgeDeadzone = 0.50f;
    public bool IsCancelled = false;

    float _pitch = 0.0f;
    float _yaw = 0.0f;
    
    public float upForce = 0.03f;
    public int upTorque = 100;
    private bool _unflip = false;
    private float _unflipStart;

    Rigidbody _rb;
    InputManager _inputManager;
    CubeController _controller;
    private Transform _cogLow;

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _inputManager = GetComponentInParent<InputManager>();
        _controller = GetComponent<CubeController>();
        _cogLow = transform.Find("cogLow");
    }

    private void FixedUpdate()
    {
        UpdateJumpVariables();
        Jump();
        JumpBackToTheFeet();
    }

    private void UpdateJumpVariables()
    {
        bool carIsGrounded = _controller.carState == CubeController.CarStates.AllWheelsSurface || _controller.carState == CubeController.CarStates.AllWheelsGround;
        if(carIsGrounded)
        {
            _isSecondJumpUsed = false;
            _lowerSecondJumpTimer = false;
            _timerSecondJump = 1.25f;
        }

        if (_lowerSecondJumpTimer)
        {
            _timerSecondJump -= Time.deltaTime;
        }


        if(IsFirstJump)
        {
            _timerJumpButtonHeld -= Time.deltaTime;
        }

        /*
        if (_rb.velocity.y > 2.6 && _rb.velocity.y < 2.8)
        {
            Debug.Log("------------------------");
            Debug.Log(_rb.velocity.y);
            Debug.Log(IsFirstJump);
            Debug.Log(_inputManager.isJump);
            Debug.Log(_timerJumpButtonHeld);
        }
        */
        
        if(IsFirstJump && (_timerJumpButtonHeld <= 0f || !_inputManager.isJump))
        {
            IsFirstJump = false;
            _lowerSecondJumpTimer = true;
            _timerSecondJump = 1.25f;
        }

        if (!IsFirstJump && carIsGrounded && _inputManager.isJump && _lastFrameNoButton)
        {
            _isFirstJumpPress = true;
            IsFirstJump = true;
            _timerJumpButtonHeld = 0.2f;
        }

        if(!IsFirstJump && !carIsGrounded && _inputManager.isJump && !_isSecondJumpUsed && _timerSecondJump > 0f && _lastFrameNoButton)
        {
            IsSecondJump = true;
            if(Mathf.Abs(_inputManager.yawInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.rollInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.pitchInput) > _dodgeDeadzone)
            {
                IsDodge = true;
                TimerDodge = 0f;
            }
        }

        if (IsDodge && TimerDodge >= 0.04f && Math.Sign(_pitch) != Math.Sign(_inputManager.pitchInput) && Mathf.Abs(_inputManager.pitchInput) > 0.999f)
        {
            IsCancelled = true;
        }
        else
        {
            IsCancelled = false;
        }

        _lastFrameNoButton = !_inputManager.isJump;
    }

    private void Jump()
    {
        //First Jump
        if(IsFirstJump)
        {
            if(_isFirstJumpPress)
            {
                _isFirstJumpPress = false;
                _rb.AddForce(_cogLow.up * 2.99f, ForceMode.VelocityChange);
            }
            else
            {
                _rb.AddForce(_cogLow.up * 13.9f, ForceMode.Acceleration);
            }
        }
        

        //Second Jump
        if(IsSecondJump)
        {
            if (IsDodge)
            {   

                if(!_isSecondJumpUsed)
                {
                    Debug.Log("Dodge detected" );

                    _pitch = _inputManager.pitchInput;
                    if(Mathf.Abs(_inputManager.rollInput) < Mathf.Abs(_inputManager.yawInput))
                    {
                        _yaw = _inputManager.yawInput;
                    }
                    else
                    {
                        _yaw = -_inputManager.rollInput;
                    }
                    AddDodgeVelocity();
                }
                AddTorque();
                if (TimerDodge >= 0.15f && TimerDodge <= 0.65f )
                {
                    _rb.velocity = Vector3.Scale(_rb.velocity, new Vector3(1f, 0.65f, 1f));
                }
                if(TimerDodge >= 0.65f)
                {
                    IsDodge = false;
                    IsSecondJump = false; 
                }
                TimerDodge += Time.deltaTime;
            }
            else
            { 
                IsSecondJump = false;
                _rb.AddForce(_cogLow.up * 2.92f, ForceMode.VelocityChange);
            }
            _isSecondJumpUsed = true;
        }
    }

    private void AddTorque()
    {
        if (!IsCancelled)
        {
            _rb.AddTorque(_cogLow.right * (220.0f * _pitch), ForceMode.Acceleration);
        }
        _rb.AddTorque(-_cogLow.forward * (220.0f * _yaw), ForceMode.Acceleration);
    }
    
    private void AddDodgeVelocity()
    {
        Vector3 forwardVelocity = _rb.velocity;
        forwardVelocity.y = 0;

        Vector3 forwardDirection = forwardVelocity.normalized;
        Vector3 sidewardDirection = (Quaternion.Euler(0.0f, 90.0f, 0.0f) * forwardVelocity).normalized;

        Vector3 carForwardDirection = _cogLow.forward;
        carForwardDirection.y = 0;

        if (forwardVelocity.magnitude == 0.0f)
        {
            forwardDirection = carForwardDirection;
            sidewardDirection = Quaternion.Euler(0.0f, 90.0f, 0.0f) * forwardDirection;
        }
        Vector3 input = new Vector3(_pitch, 0.0f, _yaw);
        float angle = Vector3.SignedAngle(input, new Vector3(1.0f, 0.0f, 0.0f), Vector3.up);
        Vector3 inputCarAligned = Quaternion.Euler(0.0f, angle, 0.0f) * carForwardDirection;
        float angleInputVelocity = Vector3.SignedAngle(inputCarAligned, forwardDirection, Vector3.up);


        if (Mathf.Abs(angleInputVelocity) < 90)
        {
            _rb.velocity += forwardDirection * Vector3.Dot(inputCarAligned, forwardDirection) * 5.0f;
        }
        if (Mathf.Abs(angleInputVelocity) > 90)
        {
            _rb.velocity += forwardDirection * Vector3.Dot(inputCarAligned, forwardDirection) * 5.33f * (1.0f + 1.5f * forwardVelocity.magnitude / 23.0f);
        }
        _rb.velocity += sidewardDirection * Vector3.Dot(inputCarAligned, sidewardDirection) * 5.0f * (1.0f + 0.9f * forwardVelocity.magnitude / 23.0f);

        if (_rb.velocity.magnitude >= 23.0f)
        {
            _rb.velocity = _rb.velocity.normalized * 23.0f;
        }
    }

    /// <summary>
    /// Turns the car back on it's "feet" if the car is currently laying on it's roof.
    /// Unflipping is done by applying a max. torque for 0.37 seconds and then letting it fall off afterwards.
    /// </summary>
    void JumpBackToTheFeet()
    {
        if (_controller.carState == CubeController.CarStates.BodyGroundDead && (_inputManager.isJumpDown || Input.GetButtonDown("A")))
        {
            _rb.AddForce(Vector3.up * upForce, ForceMode.VelocityChange);
            _rb.AddTorque(-transform.forward * upTorque, ForceMode.VelocityChange);
            _unflip = true;
            _unflipStart = 0.0f;
        }

        if (_unflip)
        {
            if (_unflipStart + Time.deltaTime < 0.37f)
            {
                _unflipStart += Time.deltaTime;
                _rb.AddTorque(-transform.forward * upTorque, ForceMode.VelocityChange);
            }
            else
            {
                _unflip = false;
                _unflipStart = 0.0f;
            }
        }
    }
}