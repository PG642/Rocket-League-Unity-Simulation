using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeJumping : MonoBehaviour
{

    float _timerJumpButtonHeld = 0;
    float _timerSecondJump = 0;

    bool _isFirstJumpPress = false;
    bool _isFirstJump = false;
    bool _isSecondJump = false;
    bool _isSecondJumpUsed = false;
    bool _lowerSecondJumpTimer = false;
    bool _lastFrameNoButton = true;

    bool _isDodge = false;
    float _timerDodge = 0f;
    const float _dodgeDeadzone = 0.50f;

    float _pitch = 0.0f;
    float _yaw = 0.0f;

    Rigidbody _rb;
    InputManager _inputManager;
    CubeController _controller;

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _inputManager = GetComponentInParent<InputManager>();
        _controller = GetComponent<CubeController>();
    }

    private void FixedUpdate()
    {
        UpdateJumpVariables();
        Jump();
        //JumpBackToTheFeet();
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


        if(_isFirstJump)
        {
            _timerJumpButtonHeld -= Time.deltaTime;
        }
        
        if(_isFirstJump && (_timerJumpButtonHeld <= 0f || !_inputManager.isJump))
        {
            _isFirstJump = false;
            _lowerSecondJumpTimer = true;
            _timerSecondJump = 1.25f;
        }

        if (!_isFirstJump && carIsGrounded && _inputManager.isJump && _lastFrameNoButton)
        {
            _isFirstJumpPress = true;
            _isFirstJump = true;
            _timerJumpButtonHeld = 0.2f;
        }

        if(!_isFirstJump && !carIsGrounded && _inputManager.isJump && !_isSecondJumpUsed && _timerSecondJump > 0f && _lastFrameNoButton)
        {
            _isSecondJump = true;
            if(Mathf.Abs(_inputManager.yawInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.rollInput) > _dodgeDeadzone || Mathf.Abs(_inputManager.pitchInput) > _dodgeDeadzone)
            {
                _isDodge = true;
                _timerDodge = 0f;
            }
        }

        _lastFrameNoButton = !_inputManager.isJump;
    }

    private void Jump()
    {
        //First Jump
        if(_isFirstJump)
        {
            if(_isFirstJumpPress)
            {
                _isFirstJumpPress = false;
                _rb.AddForce(transform.up * 2.92f, ForceMode.VelocityChange);
            }
            else
            {
                _rb.AddForce(transform.up * 14.58f, ForceMode.Acceleration);
            }
        }

        //Second Jump
        if(_isSecondJump)
        {
            if (_isDodge)
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
                if (_timerDodge >= 0.15f && _timerDodge <= 0.65f )
                {
                    _rb.velocity = Vector3.Scale(_rb.velocity, new Vector3(1f, 0.65f, 1f));
                }
                if(_timerDodge >= 0.65f)
                {
                    _isDodge = false;
                    _isSecondJump = false;
                }
                _timerDodge += Time.deltaTime;
            }
            else
            { 
                _isSecondJump = false;
                _rb.AddForce(transform.up * 2.92f, ForceMode.VelocityChange);
            }
            _isSecondJumpUsed = true;
        }
    }

    private void AddTorque()
    {
        _rb.AddTorque(-_rb.transform.forward * 47f * _yaw, ForceMode.VelocityChange);
        _rb.AddTorque(_rb.transform.right * 47f * _pitch, ForceMode.VelocityChange);
    }
    private void AddDodgeVelocity()
    {
        Vector3 forwardVelocity = _rb.velocity;
        forwardVelocity.y = 0;

        Vector3 forwardDirection = forwardVelocity.normalized;
        Vector3 sidewardDirection = (Quaternion.Euler(0.0f, 90.0f, 0.0f) * forwardVelocity).normalized;

        Vector3 carForwardDirection = transform.forward;
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

        /*// Do initial jump impulse only once
        // TODO: Currently bugged, should be .isJumpDown for the initial jump impulse.
        // Right now does the whole jump impulse
        if (_inputManager.isJump && _isCanFirstJump)
        {
            _rb.AddForce(transform.up * 292 / 100 * jumpForceMultiplier, ForceMode.VelocityChange);
            _isCanKeepJumping = true;
            _isCanFirstJump = false;
            _isJumping = true;
            
            _jumpTimer += Time.fixedDeltaTime;
        }
        
        // Keep jumping if the jump button is being pressed
        if (_inputManager.isJump && _isJumping && _isCanKeepJumping && _jumpTimer <= 0.2f)
        {
            _rb.AddForce(transform.up * 1458f / 100 * jumpForceMultiplier, ForceMode.Acceleration);
            _jumpTimer += Time.fixedDeltaTime;
        }
        
        // If jump button was released we can't start jumping again mid air
        if (_inputManager.isJumpUp)
            _isCanKeepJumping = false;
        
        // Reset jump flags when landed
        if (_controller.isAllWheelsSurface)
        {
            // Need a timer, otherwise while jumping we are setting isJumping flag to false right on the next frame 
            if (_jumpTimer >= 0.1f)
                _isJumping = false;

            _jumpTimer = 0;
            _isCanFirstJump = true;
        }
        // Cant start jumping while in the air
        else if (!_controller.isAllWheelsSurface)
            _isCanFirstJump = false;
    }

    //Auto jump and rotate when the car is on the roof
    void JumpBackToTheFeet()
    {
        if (_controller.carState != CubeController.CarStates.BodyGroundDead) return;
        
        if (_inputManager.isJumpDown || Input.GetButtonDown("A"))
        {
            _rb.AddForce(Vector3.up * upForce, ForceMode.VelocityChange);
            _rb.AddTorque(transform.forward * upTorque, ForceMode.VelocityChange);
        }
    }*/
}