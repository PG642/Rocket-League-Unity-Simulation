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
    private Scenario _currentScenario;

    private float _nextActionTime;
    private bool _done = false;
    private TestLogger _logger;

    // Start is called before the first frame update
    void Start()
    {
        var fromJson = JsonUtility.FromJson<RootJson>(jsonFile.text);
        _currentScenario = fromJson.scenarios[0];
        SetupCar(_currentScenario);
        SetupBall(_currentScenario);
        _actions = _currentScenario.actions;
        _nextActionTime = 1.0f;
        GetInputManager();
        var car_rb = GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "ControllableCar").FirstOrDefault();

        _logger = new TestLogger(car_rb,_currentScenario);
    }

    private void GetInputManager()
    {
        _inputManager = GetComponentsInChildren<InputManager>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        if (_inputManager != null) _inputManager.isAgent = true;
    }

    private void SetupCar(Scenario scenario)
    {
        var carStartValue = scenario.startValues.Find(x => x.gameObject == "car");
        var car_rb = GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        SetupObject(carStartValue,car_rb,0.1701f);
    }

    private void SetupBall(Scenario scenario)
    {
        var ballStartValue = scenario.startValues.Find(x => x.gameObject == "ball");
        var ball_rb = GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "Ball").FirstOrDefault();
        SetupObject(ballStartValue,ball_rb,0.9275f);
    }

    private void SetupObject(GameObjectValue gameObjectValue, Rigidbody rigidbody, float offsetY = 0.0f)
    {
        rigidbody.position = gameObjectValue.position.ToVector(offsetY:offsetY);
        rigidbody.rotation = gameObjectValue.rotation.ToQuaternion();
        rigidbody.velocity = gameObjectValue.velocity.ToVector();
        rigidbody.angularVelocity = gameObjectValue.angularVelocity.ToVector();
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
                Debug.LogError("Input could not be mapped");
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
                _currentAction = _actions.First();
                _nextActionTime = Time.time + _currentAction.duration;
                _actions.RemoveAt(0);
                ApplyActionOnCar(_currentAction);
            }
            else
            {
                ResetActionCar();
            }
            
        }

        if (Time.time > _currentScenario.time && !_done)
        {
            _done = true;
            _logger.SaveLog();
        }

        _logger?.Log();
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