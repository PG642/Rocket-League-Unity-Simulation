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
        
        // reset timer and second jump if the car is grounded 
        if(carIsGrounded)
        {
            _isSecondJumpUsed = false;
            _lowerSecondJumpTimer = false;
            _timerSecondJump = 1.25f;
        }

        // lower _timerSecondJump if the car is in air
        if (_lowerSecondJumpTimer)
        {
            _timerSecondJump -= Time.deltaTime;
        }

        // lower _timerJumpButtonHeld if the car is in air and the first jump is active
        if (IsFirstJump)
        {
            _timerJumpButtonHeld -= Time.deltaTime;
        }

        // if the interval of the first jump ended or the button was released: end first jump, start lowering the secondjumptimer from here and reset it before lowering it
        if(IsFirstJump && (_timerJumpButtonHeld <= 0f || !_inputManager.isJump))
        {
            IsFirstJump = false;
            _lowerSecondJumpTimer = true;
            _timerSecondJump = 1.25f;
        }

        // if the car is grounded, is not already jumping and the jump button is pressed again: start first jump, reset _timerJumpButtonHeld
        if (!IsFirstJump && carIsGrounded && _inputManager.isJump && _lastFrameNoButton)
        {
            _isFirstJumpPress = true;
            IsFirstJump = true;
            _timerJumpButtonHeld = 0.195f;
        }

        // if the first jump is over, the car is not grounded and has the second jump left: start second jump / dodge
        if(!IsFirstJump && !carIsGrounded && _inputManager.isJump && !_isSecondJumpUsed && _timerSecondJump > 0f && _lastFrameNoButton)
        {
            IsSecondJump = true;
            // if the dodge deadzone is exceeded the second jump is a dodge: start dodge, reset TimerDodge and raise the maxAngularVelocity during the dodge
            if (Mathf.Abs(_inputManager.yawInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.rollInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.pitchInput) > _dodgeDeadzone)
            {
                IsDodge = true;
                _rb.maxAngularVelocity = 7.3f;
                TimerDodge = 0f;
            }
        }

        // dodges can be cancelled by pulling the stick in the opposit pitch direction
        if (IsDodge && TimerDodge >= 0.04f && Math.Sign(_pitch) != Math.Sign(_inputManager.pitchInput) && Mathf.Abs(_inputManager.pitchInput) > 0.999f)
        {
            IsCancelled = true;
        }
        else
        {
            IsCancelled = false;
        }

        // track if the jump putton was pressed in the last frame
        _lastFrameNoButton = !_inputManager.isJump;
    }

    private void Jump()
    {
        //First Jump
        if(IsFirstJump)
        {
            // add initial forces
            if(_isFirstJumpPress)
            {
                _isFirstJumpPress = false;
                _rb.AddForce(_rb.transform.up * 2.99f, ForceMode.VelocityChange);
            }
            // add acceleration if the jump button is held down
            else
            {
                _rb.AddForce(_cogLow.up * 13.9f, ForceMode.Acceleration);
            }
        }
        

        //Second Jump
        if(IsSecondJump)
        {
            // start dodge routine
            if (IsDodge)
            {
                // only if the second jump is not used in an earlier frame: get the direction of the dodge
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
                // add torque over the full dodge time
                AddTorque();
                if (TimerDodge >= 0.15f && TimerDodge <= 0.65f )
                {
                    _rb.velocity = Vector3.Scale(_rb.velocity, new Vector3(1f, 0.65f, 1f));
                }
                // stop the dodge after the dodge time was exceeded and restore the default maxAngularVelocity
                if(TimerDodge >= 0.65f)
                {
                    IsDodge = false;
                    _rb.maxAngularVelocity = 5.5f;
                    IsSecondJump = false; 
                }
                TimerDodge += Time.deltaTime;
            }
            // add initial forces for a non-dodge second jump
            else
            { 
                IsSecondJump = false;
                _rb.AddForce(_rb.transform.up * 2.92f, ForceMode.VelocityChange);
            }
            _isSecondJumpUsed = true;
        }
    }

    private void AddTorque()
    {
        // only add pitch torque if the dodge is not cancelled
        if (!IsCancelled)
        {
            _rb.AddTorque(-_cogLow.right * (220.0f * _pitch), ForceMode.Acceleration);
        }
        // add roll torque anyway
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

        // only differ between the orientation of the car and its velocity direction, if there is a noticeable velocity
        if (forwardVelocity.magnitude <= 1.0e-4)
        {
            forwardDirection = carForwardDirection;
            sidewardDirection = Quaternion.Euler(0.0f, 90.0f, 0.0f) * forwardDirection;
        }
        // transform the input direction (aligned with the car) to the global space to compare it to the cars velocity directions
        Vector3 input = new Vector3(-_pitch, 0.0f, _yaw);
        float angle = Vector3.SignedAngle(input, new Vector3(1.0f, 0.0f, 0.0f), Vector3.up);
        Vector3 inputCarAligned = Quaternion.Euler(0.0f, angle, 0.0f) * carForwardDirection;
        float angleInputVelocity = Vector3.SignedAngle(inputCarAligned, forwardDirection, Vector3.up);

        // add different dodge velocities for different kinds of dodges
        // forward dodge
        if (Mathf.Abs(angleInputVelocity) < 90)
        {
            _rb.velocity += forwardDirection * Vector3.Dot(inputCarAligned, forwardDirection) * 5.0f;
        }
        // backward dodge
        else
        {
            _rb.velocity += forwardDirection * Vector3.Dot(inputCarAligned, forwardDirection) * 5.33f * (1.0f + 1.5f * forwardVelocity.magnitude / 23.0f);
        }
        // sideward dodge
        _rb.velocity += sidewardDirection * Vector3.Dot(inputCarAligned, sidewardDirection) * 5.0f * (1.0f + 0.9f * forwardVelocity.magnitude / 23.0f);

        // limit the speed of the car directly after adding the velocity to keep consistency
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
        if (_unflip)
        {
            if (_unflipStart + Time.deltaTime < 0.33f)
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
        
        if (_controller.carState == CubeController.CarStates.BodyGroundDead && (_inputManager.isJump || Input.GetButtonDown("A")))
        {
            _rb.AddForce(Vector3.up * upForce, ForceMode.VelocityChange);
            _rb.AddTorque(-transform.forward * upTorque, ForceMode.VelocityChange);
            _unflip = true;
            _unflipStart = 0.0f;
        }
    }

    public void Reset()
    {
        // add reset logic to restore default values (e.g. OnEpisodeBegin() for training)
        _timerJumpButtonHeld = 0;
        _timerSecondJump = 0;

        _isFirstJumpPress = false;
        IsFirstJump = false;
        IsSecondJump = false;
        _isSecondJumpUsed = false;
        _lowerSecondJumpTimer = false;
        _lastFrameNoButton = true;

        IsDodge = false;
        TimerDodge = 0f;
        IsCancelled = false;

        _pitch = 0.0f;
        _yaw = 0.0f;
    }

}