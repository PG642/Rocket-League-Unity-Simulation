using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using DefaultNamespace;
using JsonObjects;
using UnityEngine;
using Action = JsonObjects.Action;
using Input = JsonObjects.Input;

public class TestController : MonoBehaviour
{
    public TextAsset jsonFile;

    private List<Action> _actions;
    private InputManager _inputManager;
    private Action _currentAction;

    private float _nextActionTime;

    // Start is called before the first frame update
    void Start()
    {
        var fromJson = JsonUtility.FromJson<RootJson>(jsonFile.text);
        var firstScenario = fromJson.scenarios[0];
        SetupCar(firstScenario);
        SetupBall(firstScenario);
        _actions = firstScenario.actions;
        _nextActionTime = 1.0f;
        GetInputManager();
    }

    private void GetInputManager()
    {
        _inputManager = GetComponentsInChildren<InputManager>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        _inputManager.isAgent = true;
    }

    private void SetupCar(Scenario scenario)
    {
        var carStartValue = scenario.startValues.Find(x => x.gameObject == "car");
        var rotation = Quaternion.Euler(carStartValue.rotation.x + 0.55f, carStartValue.rotation.y,
            carStartValue.rotation.z);
        var position = new Vector3(carStartValue.position.x, carStartValue.position.y + 0.1701f,
            carStartValue.position.z);
        var car_rb = GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        car_rb.position = carStartValue.position.ToVector( offsetY:0.1701f);
        car_rb.rotation = carStartValue.rotation.ToQuaternion();
        car_rb.velocity = carStartValue.velocity.ToVector();
        car_rb.angularVelocity = carStartValue.angularVelocity.ToVector();
    }

    private void SetupBall(Scenario scenario)
    {
        var ballStartValue = scenario.startValues.Find(x => x.gameObject == "ball");
        var ball_rb = GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "Ball").FirstOrDefault();
        ball_rb.position = ballStartValue.position.ToVector(offsetY:0.9275f);
        ball_rb.rotation = ballStartValue.rotation.ToQuaternion();
        ball_rb.velocity = ballStartValue.velocity.ToVector();
        ball_rb.angularVelocity = ballStartValue.angularVelocity.ToVector();
    }
    private void ApplyActionOnCar(Action nextAction)
    {
        if (nextAction == null) return;
        var inputs = nextAction.inputs;
        inputs.ForEach(input => ApplyInput(input));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ApplyInput(Input input)
    {
        switch (input.name)
        {
            case "jump":
                _inputManager.isJump = Convert.ToBoolean(input.value);
                break;
            case "boost":
                _inputManager.isBoost = Convert.ToBoolean(input.value);
                break;
            case "handbrake":
                _inputManager.isDrift = Convert.ToBoolean(input.value);
                break;
            case "steer":
                _inputManager.steerInput = input.value;
                break;
            case "pitch":
                _inputManager.pitchInput = input.value;
                break;
            case "yaw":
                _inputManager.yawInput = input.value;
                break;
            case "roll":
                _inputManager.rollInput = input.value;
                break;
            case "throttle":
                _inputManager.throttleInput = input.value;
                break;
            default:
                Debug.LogWarning("Input could not be mapped");
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time <= _nextActionTime)
        {
            ApplyActionOnCar(_currentAction);
        }

        if (Time.time > _nextActionTime)
        {
            if (_actions.Count > 0)
            {
                ResetActionCar();
                _currentAction = _actions[0];
                _nextActionTime = Time.time + _currentAction.duration;
                _actions.RemoveAt(0);
                ApplyActionOnCar(_currentAction);
            }
            else
            {
                ResetActionCar();
            }
        }
    }

    private void ResetActionCar()
    {
        _inputManager.isJump = false;
        _inputManager.isBoost = false;
        _inputManager.isDrift = false;
        _inputManager.steerInput = 0.0f;
        _inputManager.pitchInput = 0.0f;
        _inputManager.yawInput = 0.0f;
        _inputManager.rollInput = 0.0f;
        _inputManager.throttleInput = 0.0f;
    }
}