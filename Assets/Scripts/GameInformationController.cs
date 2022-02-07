using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInformationController : MonoBehaviour
{
    // Start is called before the first frame update
    public float boost;
    public bool wheelsOnGround;
    public bool jumped;
    private CubeBoosting _cubeBoosting;
    private CubeController _cubeController;
    private CubeJumping _jumpController;

    void Start()
    {
        _cubeBoosting= GetComponentInChildren<CubeBoosting>();
        _cubeController = GetComponentInChildren<CubeController>();
        _jumpController = GetComponentInChildren<CubeJumping>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        boost = _cubeBoosting.boostAmount;
        wheelsOnGround = _cubeController.isAllWheelsSurface;
        jumped = _jumpController.isFirstJump || _jumpController.isSecondJump;
    }

    public void SetStartValues(float boostAmount)
    {
        if (boostAmount > 0)
        GetComponentInChildren<CubeBoosting>().boostAmount = boostAmount;

        GetComponentInChildren<CubeBoosting>().infiniteBoosting = boostAmount < 0;
    }
}
