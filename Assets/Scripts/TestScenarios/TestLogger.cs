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
        private Log _currentLog;
        private readonly Rigidbody _rigidbodyBall;

        public TestLogger(Rigidbody rigidbodyCar,Rigidbody rigidbodyBall, JsonClasses.Scenario scenario, InputManager inputManager)
        {
            _rigidbodyCar = rigidbodyCar;
            _rigidbodyBall = rigidbodyBall;
            _inputManager = inputManager;

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
                carValue = new GameCar()
                {
                    position = _rigidbodyCar.position.ToVector(-0.1700f),
                    velocity = _rigidbodyCar.velocity.ToVector(),
                    rotation = _rigidbodyCar.rotation.ToVector(),
                    boost = boost,
                    hasWheelContact = wheelsOnGround,
                    jumped = jumped,
                },
                ballValue = new BallValues()
                {
                    position = _rigidbodyBall.position.ToVector(-0.9275f),
                    velocity =  _rigidbodyBall.velocity.ToVector(),
                    rotation = _rigidbodyBall.rotation.ToVector(),
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
            string path = Application.persistentDataPath + $"/{log.name}.json";
            using (StreamWriter file = new StreamWriter(path))
            {
                string msg = JsonUtility.ToJson(log, true);
                file.Write(msg);
                Debug.Log($"Saved{log.name} at {path}");
            }
        }
    }
}