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

        public TestLogger(Rigidbody rigidbodyCar, JsonClasses.Scenario scenario, InputManager inputManager)
        {
            _rigidbodyCar = rigidbodyCar;
            _inputManager = inputManager;

            StartNewLogging(scenario.name);
        }

        public void StartNewLogging(string scenarioName)
        {
            _currentLog = new Log()
            {
                name = scenarioName,
                logValues = new List<LogValue>()
            };
        }
        
        public void Log()
        {
            var logValue = new LogValue()
            {
                time = Time.time,
                gameObjectValue = new GameObjectSaveValue()
                {
                    gameObject = "car",
                    position = _rigidbodyCar.position.ToVector(),
                    velocity = _rigidbodyCar.velocity.ToVector(),
                    rotation = _rigidbodyCar.rotation.ToVector(),
                }
                
            };
            _currentLog.logValues.Add(logValue);
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