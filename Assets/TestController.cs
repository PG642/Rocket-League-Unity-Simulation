using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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
        Debug.Log(fromJson);
        SetupCar(fromJson);
        _actions = fromJson.scenarios[0].actions;
        _nextActionTime = 1.0f;
    }

    private void SetupCar(RootJson fromJson)
    {
        var car = transform.Find("ControllableCar");
        var carStartValue = fromJson.scenarios[0].startValues.Find(x => x.gameObject == "car");
        var rotation = Quaternion.Euler(carStartValue.rotation.x + 0.55f, carStartValue.rotation.y, carStartValue.rotation.z);
        var position = new Vector3(carStartValue.position.x, carStartValue.position.y +0.1701f, carStartValue.position.z);
        var car_rb =GetComponentsInChildren<Rigidbody>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        _inputManager = GetComponentsInChildren<InputManager>().Where(x => x.tag == "ControllableCar").FirstOrDefault();
        car_rb.position = position;
        car_rb.rotation = rotation;
        car_rb.velocity = new Vector3(carStartValue.velocity.x, carStartValue.velocity.y, carStartValue.velocity.z);
    }

    private void ApplyActionOnCar(Action nextAction)
    {
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
    void Update()
    {
        if (Time.time <= _nextActionTime)
        {
            ApplyActionOnCar(_currentAction);
        }
        if (Time.time > _nextActionTime && _actions.Count > 0)
        {
            _currentAction = _actions[0];
            _nextActionTime = Time.time + _currentAction.duration; 
            _actions.RemoveAt(0);
            ApplyActionOnCar(_currentAction);
        }
    }
}