using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extentions;
using TestScenarios.JsonClasses;
using UnityEngine;

namespace TestScenarios
{
    public class TestLogger
    {
        private List<GameObject> _controllableCars;
        private Rigidbody _rigidbodyBall;
        private readonly string _loggingPath;
        private Log _currentLog;


        private Dictionary<GameObject, Rigidbody> _dictCarRb;
        private Dictionary<GameObject, string> _dictCarId;
        private Dictionary<GameObject, CubeBoosting> _dictCarBoosting;
        private Dictionary<GameObject, CubeController> _dictCarController;
        private Dictionary<GameObject, CubeJumping> _dictCarJumping;

        

        public TestLogger(JsonClasses.Scenario scenario, string loggingPath)
        {
            _dictCarRb = new Dictionary<GameObject, Rigidbody>();
            _dictCarId = new Dictionary<GameObject, string>();
            _dictCarBoosting = new Dictionary<GameObject, CubeBoosting>();
            _dictCarController = new Dictionary<GameObject, CubeController>();
            _dictCarJumping = new Dictionary<GameObject, CubeJumping>();
            _controllableCars = new List<GameObject>();
            _loggingPath = loggingPath;

            StartNewLogging(scenario.name);
        }

        public void AddControllableCar(GameObject controllableCar, string id)
        {
            _controllableCars.Add(controllableCar);
            _dictCarId.Add(controllableCar, id);
            _dictCarRb.Add(controllableCar, controllableCar.GetComponent<Rigidbody>());
            _dictCarBoosting.Add(controllableCar, controllableCar.GetComponentInChildren<CubeBoosting>());
            _dictCarController.Add(controllableCar, controllableCar.GetComponentInChildren<CubeController>());
            _dictCarJumping.Add(controllableCar, controllableCar.GetComponentInChildren<CubeJumping>());
        }

        public void SetRigidbodyBall(Rigidbody rigidbodyBall)
        {
            _rigidbodyBall = rigidbodyBall;
        }

        public void StartNewLogging(string scenarioName)
        {
            _currentLog = new Log()
            {
                name = scenarioName,
                frames = new List<LogValue>()
            };
        }

        public void Log()
        {
            var logValue = new LogValue()
            {
                time = Time.time,
                game_cars = new List<GameCar>(
                    from controllableCar in _controllableCars select
                    new GameCar()
                    {
                        id = _dictCarId[controllableCar],
                        physics = new GameObjectSaveValue()
                        {
                            location = _dictCarRb[controllableCar].position.ToVector(),
                            velocity = _dictCarRb[controllableCar].velocity.ToVector(),
                            rotation = _dictCarRb[controllableCar].rotation.ToVector(),
                            angular_velocity = _dictCarRb[controllableCar].angularVelocity.ToVector()
                        },

                        boost = _dictCarBoosting[controllableCar]._boostAmount,
                        has_wheel_contact = _dictCarController[controllableCar].isAllWheelsSurface,
                        jumped = _dictCarJumping[controllableCar].IsFirstJump || _dictCarJumping[controllableCar].IsSecondJump
                    }),
                game_ball = new BallValues()
                {
                    physics = new GameObjectSaveValue()
                    {
                        location = _rigidbodyBall.position.ToVector(),
                        velocity = _rigidbodyBall.velocity.ToVector(),
                        rotation = _rigidbodyBall.rotation.ToVector(),
                        angular_velocity = _rigidbodyBall.angularVelocity.ToVector()
                    }
                }
            };
            _currentLog.frames.Add(logValue);
        }

        public void SaveLog()
        {
            SaveAsJson(_currentLog);
        }

        private void SaveAsJson(Log log)
        {
            string json = JsonUtility.ToJson(log);
            string path = _loggingPath + $"{log.name}.json";
            using (StreamWriter file = new StreamWriter(path))
            {
                string msg = JsonUtility.ToJson(log, true);
                file.Write(msg);
                Debug.Log($"Saved{log.name} at {path}");
            }
        }
    }
}