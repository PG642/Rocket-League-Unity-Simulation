using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extentions;
using TestScenarios.JsonClasses;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Action = TestScenarios.JsonClasses.Action;
using File = UnityEngine.Windows.File;
using Input = TestScenarios.JsonClasses.Input;
using Scenario = TestScenarios.JsonClasses.Scenario;

namespace TestScenarios
{
    public class TestController : MonoBehaviour
    {
        public TextAsset jsonFile;

        public string filePath =
            @"paht\to\roboleague\dir";

        public string subdir = @"Szenarien\";
        public string fileName = "Test.json";

        private List<Action> _actions;
        private InputManager _inputManager;
        private Action _currentAction;
        private Scenario _currentScenario;
        private List<Scenario> _scenarios;
        private float _nextActionTime;
        private bool _done = false;
        private TestLogger _logger;

        private GameInformationController _gameInformationController;

        void Start()
        {
            _gameInformationController = GetComponent<GameInformationController>();
            if (!System.IO.File.Exists(filePath + subdir + fileName))
            {
                return;
            }

            var test = System.IO.File.ReadAllText(filePath + subdir + fileName);

            var fromJson = JsonUtility.FromJson<Scenario>(test);
            _currentScenario = fromJson;
            var carRb = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x => x.CompareTag("ControllableCar"));
            var ballRb = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x => x.CompareTag("Ball"));
            SetupCar(_currentScenario, carRb);
            SetupBall(_currentScenario, ballRb);
            GetInputManager();
            _actions = _currentScenario.actions;
            _nextActionTime = 1.0f;
            _gameInformationController.SetStartValues(_currentScenario.boost);


            _logger = new TestLogger(carRb, ballRb, _currentScenario, _inputManager, filePath);
        }

        private void GetInputManager()
        {
            _inputManager = GetComponentsInChildren<InputManager>()
                .FirstOrDefault(x => x.CompareTag("ControllableCar"));
            if (_inputManager != null) _inputManager.isAgent = true;
        }

        private void SetupCar(Scenario scenario, Rigidbody carRb)
        {
            var carStartValue = scenario.startValues.Find(x => x.gameObject == "car");
            SetupObject(carStartValue, carRb, 0.1700f);
        }

        private void SetupBall(Scenario scenario, Rigidbody ballRb)
        {
            var ballStartValue = scenario.startValues.Find(x => x.gameObject == "ball");
            SetupObject(ballStartValue, ballRb, 0.9275f);
        }

        private void SetupObject(GameObjectValue gameObjectValue, Rigidbody rigidBody, float offsetY = 0.0f)
        {
            rigidBody.position = gameObjectValue.position.ToVector(offsetY: offsetY);
            rigidBody.rotation = gameObjectValue.rotation.ToQuaternion();
            rigidBody.velocity = gameObjectValue.velocity.ToVector();
            rigidBody.angularVelocity = gameObjectValue.angularVelocity.ToVector();
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
            ExecutedScenario();
        }


        private void ExecutedScenario()
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

            if (Time.time > _currentScenario?.time && !_done)
            {
                _done = true;
                _logger.SaveLog();
            }

            _logger?.Log(_gameInformationController.boost, _gameInformationController.wheelsOnGround,
                _gameInformationController.jumped);
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
}