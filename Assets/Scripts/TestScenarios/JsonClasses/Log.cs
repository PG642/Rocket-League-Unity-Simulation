using System;
using System.Collections.Generic;

namespace TestScenarios.JsonClasses
{
    [Serializable]
    public class Log
    {
        public List<LogValue> logValues;
        public string name;
    }

    [Serializable]
    public class LogValue
    {
        public float time;
        public GameObjectSaveValue gameObjectValue;
    }

    [Serializable]
    public class GameObjectSaveValue
    {
        public string gameObject;
        public SetupPoint position;
        public SetupPoint velocity;
        public SetupPoint angularVelocity;
        public SetupPoint rotation;
        public bool hasWheelContact;
        public bool jumped;
        public bool doubleJumped;
        public bool boost;
    }
    
    
}