using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JsonObjects;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace DefaultNamespace
{
    public class TestLogger
    {
        private Rigidbody _rigidbodyCar;
        private Log _currentLog;

        public TestLogger(Rigidbody rigidbodyCar, Scenario scenario)
        {
            _rigidbodyCar = rigidbodyCar;

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