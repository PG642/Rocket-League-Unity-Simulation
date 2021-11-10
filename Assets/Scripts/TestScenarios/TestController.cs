using System;
using System.Collections.Generic;
using System.Linq;
using Extentions;
using TestScenarios.JsonClasses;
using UnityEngine;
using Action = TestScenarios.JsonClasses.Action;
using Input = TestScenarios.JsonClasses.Input;
using Scenario = TestScenarios.JsonClasses.Scenario;

namespace TestScenarios
{
    public class TestController : MonoBehaviour
    {
        public TextAsset settingJson;
        public GameObject controllableCarPrefab;

        private Scenario _currentScenario;
        private List<Scenario> _scenarios;
        private bool _done = false;
        private TestLogger _logger;
        private bool _isFirstUpdate = true;

        private List<GameObject> _controllableCars = new List<GameObject>();
        private Dictionary<GameObject, InputManager> _dictCarInput = new Dictionary<GameObject, InputManager>();
        private Dictionary<GameObject, List<Action>> _dictCarAction = new Dictionary<GameObject, List<Action>>();
        private Dictionary<GameObject, float> _dictCarNextActionTime = new Dictionary<GameObject, float>();

        void Start()
        {

            var settingsToSettings = JsonUtility.FromJson<ToSettings>(settingJson.text);
            var jsonSettingsPath = settingsToSettings.settings_path;

            var jsonSettings = System.IO.File.ReadAllText(jsonSettingsPath);

            var settings = JsonUtility.FromJson<SafeSettings>(jsonSettings);
            var jsonScenarioPath = settings.szenario_path + settings.file_name;

            var scenario = System.IO.File.ReadAllText(jsonScenarioPath);

            var fromJson = JsonUtility.FromJson<Scenario>(scenario);
            _currentScenario = fromJson;
            _logger = new TestLogger(_currentScenario, settings.results_path_robo_league);



            InitializeGameobjects(_currentScenario.gameObjects);


        }

        private void InitializeGameobjects(List<GameObjectValue> gameObjects)
        { 
            List<GameObject> blueTeam = new List<GameObject>();
            List<GameObject> orangeTeam = new List<GameObject>();
            foreach (GameObjectValue gov in gameObjects)
            {
                if ("car".Equals(gov.gameObject))
                {
                    GameObject controllableCar = GameObject.Instantiate(controllableCarPrefab, transform);
                    controllableCar.GetComponentInChildren<CubeBoosting>().SetInfiniteBoost(true);
                    _controllableCars.Add(controllableCar);
                    _dictCarInput.Add(controllableCar, controllableCar.GetComponent<InputManager>());
                    _dictCarAction.Add(controllableCar, gov.actions);
                    _dictCarNextActionTime.Add(controllableCar, -1.0f);
                    Rigidbody carRb = controllableCar.GetComponent<Rigidbody>();
                    _logger.AddControllableCar(controllableCar, gov.id);
                    SetupCar(gov, carRb);
                    if ("car".Equals(gov.gameObject))
                    {
                        // generate Teams
                        if ("0".Equals(gov.team) || "blue".Equals(gov.team.ToLower()))
                        {
                            blueTeam.Add(controllableCar);
                        }
                        if ("1".Equals(gov.team) || "orange".Equals(gov.team.ToLower()))
                        {
                            orangeTeam.Add(controllableCar);
                        }
                    }
                    if (carRb.position.y > 0.1701f && Mathf.Abs(carRb.rotation.x) < 0.001f)
                    {
                        var cc = GetComponentInChildren<CubeController>();
                        cc.isCanDrive = false;
                        cc.carState = CubeController.CarStates.Air;
                        cc.isAllWheelsSurface = false;
                        cc.numWheelsSurface = 0;
                    }
                }
                else if("ball".Equals(gov.gameObject))
                {
                    var ballRb = GetComponentsInChildren<Rigidbody>().FirstOrDefault(x => x.CompareTag("Ball"));
                    SetupBall(gov, ballRb);
                    _logger.SetRigidbodyBall(ballRb);
                }
            }
            TeamController teamController = gameObject.GetComponent<TeamController>();
            teamController.SetTeams(blueTeam, orangeTeam);
        }


        private void SetupCar(GameObjectValue gov, Rigidbody carRb)
        {
            var carStartValues = gov.startValues;
            SetupObject(carStartValues, carRb);
            carRb.freezeRotation = true;
        }

        private void SetupBall(GameObjectValue gov, Rigidbody ballRb)
        {
            var ballStartValues = gov.startValues;
            SetupObject(ballStartValues, ballRb);
        }

        private void SetupObject(StartValues startValues, Rigidbody rigidBody, float offsetY = 0.0f)
        {
            rigidBody.position = startValues.position.ToVector(offsetY: offsetY);
            rigidBody.rotation = startValues.rotation.ToQuaternion();
            rigidBody.velocity = startValues.velocity.ToVector();
            rigidBody.angularVelocity = startValues.angularVelocity.ToVector();
        }

        private void ApplyActionOnCar(InputManager inputManager, Action nextAction)
        {
            if (nextAction == null) return;
            var inputs = nextAction.inputs;
            inputs.ForEach(input => ApplyInput(inputManager, input));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void ApplyInput(InputManager inputManager, Input input)
        {
            switch (input.name)
            {
                case "jump":
                    inputManager.isJump = Convert.ToBoolean(input.value);
                    break;
                case "boost":
                    inputManager.isBoost = Convert.ToBoolean(input.value);
                    break;
                case "handbrake":
                    inputManager.isDrift = Convert.ToBoolean(input.value);
                    break;
                case "steer":
                    inputManager.steerInput = input.value;
                    break;
                case "pitch":
                    inputManager.pitchInput = input.value;
                    break;
                case "yaw":
                    inputManager.yawInput = input.value;
                    break;
                case "roll":
                    inputManager.rollInput = input.value;
                    break;
                case "throttle":
                    inputManager.throttleInput = input.value;
                    break;
                default:
                    Debug.LogError("Input could not be mapped");
                    break;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(_isFirstUpdate)
            {
                foreach(GameObject controllableCar in _controllableCars)
                {
                    controllableCar.GetComponent<Rigidbody>().freezeRotation = false;
                }
                _isFirstUpdate = false;
            }
            ExecutedScenario();
        }


        private void ExecutedScenario()
        {
            foreach(GameObject controllableCar in _controllableCars)
            {
                //Debug.Log("time: " + Time.time + " nextActionTime: " + _dictCarNextActionTime[controllableCar]);
                if (Time.time <= _dictCarNextActionTime[controllableCar])
                {
                    ApplyActionOnCar(_dictCarInput[controllableCar], _dictCarAction[controllableCar].First());
                }
                else
                {
                    if (_dictCarAction[controllableCar].Count > 0)
                    {
                        ResetActionCar(_dictCarInput[controllableCar]);
                        if(_dictCarNextActionTime[controllableCar]!=-1.0)
                        {
                            _dictCarAction[controllableCar].RemoveAt(0);
                        }
                        if (_dictCarAction[controllableCar].Count > 0)
                        {
                            _dictCarNextActionTime[controllableCar] = Time.time + _dictCarAction[controllableCar].First().duration;
                            ApplyActionOnCar(_dictCarInput[controllableCar], _dictCarAction[controllableCar].First());
                        }
                        else
                        {
                            ResetActionCar(_dictCarInput[controllableCar]);
                        }
                    }
                    else
                    {
                        ResetActionCar(_dictCarInput[controllableCar]);
                    }
                }

            }

            if (Time.time > _currentScenario?.time && !_done)
            {
                _done = true;
                _logger.SaveLog();
            }

            _logger?.Log();
        }

        private void ResetActionCar(InputManager inputManager)
        {
            inputManager.isJump = false;
            inputManager.isBoost = false;
            inputManager.isDrift = false;
            inputManager.steerInput = 0.0f;
            inputManager.pitchInput = 0.0f;
            inputManager.yawInput = 0.0f;
            inputManager.rollInput = 0.0f;
            inputManager.throttleInput = 0.0f;
        }
    }

    [Serializable]
    internal class ToSettings
    {
        public string settings_path;
    }

    internal class SafeSettings
    {
        public string results_path_robo_league;
        public string szenario_path;
        public string file_name;
    }
}