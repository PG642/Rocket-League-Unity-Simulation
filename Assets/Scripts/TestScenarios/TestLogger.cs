using System.Collections.Generic;
using System.IO;
using Extentions;
using TestScenarios.JsonClasses;
using UnityEngine;

namespace TestScenarios
{
    public class TestLogger
    {
        private Rigidbody _rigidbodyCar;
        private readonly InputManager _inputManager;
        private readonly string _loggingPath;
        private Log _currentLog;
        private readonly Rigidbody _rigidbodyBall;
        

        public TestLogger(Rigidbody rigidbodyCar, Rigidbody rigidbodyBall, JsonClasses.Scenario scenario,
            InputManager inputManager, string loggingPath)
        {
            _rigidbodyCar = rigidbodyCar;
            _rigidbodyBall = rigidbodyBall;
            _inputManager = inputManager;
            _loggingPath = $@"{loggingPath}Ergebnisse\RoboLeague\";

            StartNewLogging(scenario.name);
        }

        public void StartNewLogging(string scenarioName)
        {
            _currentLog = new Log()
            {
                name = scenarioName,
                frames = new List<LogValue>()
            };
        }

        public void Log(float boost, bool wheelsOnGround, bool jumped)
        {
            var logValue = new LogValue()
            {
                time = Time.time,
                game_cars = new List<GameCar>()
                {
                    new GameCar()
                    {
                        physics = new GameObjectSaveValue()
                        {
                            location = _rigidbodyCar.position.ToVector(),
                            velocity = _rigidbodyCar.velocity.ToVector(),
                            rotation = _rigidbodyCar.rotation.ToVector(),
                        },

                        boost = boost,
                        has_wheel_contact = wheelsOnGround,
                        jumped = jumped,
                    }
                },
                game_ball = new BallValues()
                {
                    physics = new GameObjectSaveValue()
                    {
                        location = _rigidbodyBall.position.ToVector(),
                        velocity = _rigidbodyBall.velocity.ToVector(),
                        rotation = _rigidbodyBall.rotation.ToVector(),
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