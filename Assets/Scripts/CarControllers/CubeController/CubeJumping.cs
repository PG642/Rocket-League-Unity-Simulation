using UnityEngine;

[RequireComponent(typeof(CubeController))]
public class CubeJumping : MonoBehaviour
{
    [Header("Forces")]
    [Range(0.25f,4)]
    // default 1
    public float jumpForceMultiplier = 1f;
    public float upForce = 0.03f;
    public int upTorque = 100;
    
    float _jumpTimer = 0;
    [SerializeField]
    bool _isCanFirstJump = false;
    public bool _isJumping = false;
    [SerializeField]
    bool _isCanKeepJumping = false;

    Rigidbody _rb;
    InputManager _inputManager;
    CubeController _controller;

    private bool _unflip = false;
    private float _unflipStart;

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _inputManager = GetComponentInParent<InputManager>();
        _controller = GetComponent<CubeController>();
    }

    private void FixedUpdate()
    {
        Jump();
        JumpBackToTheFeet();
    }
    
    private void Jump()
    {
        // Do initial jump impulse only once
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

    /// <summary>
    /// Turns the car back on it's "feet" if the car is currently laying on it's roof.
    /// Unflipping is done by applying a max. torque for 0.37 seconds and then letting it fall off afterwards.
    /// </summary>
    void JumpBackToTheFeet()
    {
        if (_controller.carState == CubeController.CarStates.BodyGroundDead && (_inputManager.isJumpDown || Input.GetButtonDown("A")))
        {
            _rb.AddForce(Vector3.up * upForce, ForceMode.VelocityChange);
            _rb.AddTorque(transform.forward * upTorque, ForceMode.VelocityChange);
            _unflip = true;
            _unflipStart = 0.0f;
        }

        if (_unflip)
        {
            if (_unflipStart + Time.deltaTime < 0.37f)
            {
                _unflipStart += Time.deltaTime;
                _rb.AddTorque(transform.forward * upTorque, ForceMode.VelocityChange);
            }
            else
            {
                _unflip = false;
                _unflipStart = 0.0f;
            }
        }
    }
}