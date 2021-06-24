using System;
using System.Collections.Generic;

namespace TestScenarios.JsonClasses
{
    [Serializable]
    public class SetupPoint
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class GameObjectValue
    {
        public string gameObject;
        public SetupPoint position;
        public SetupPoint velocity;
        public SetupPoint angularVelocity;
        public SetupPoint rotation;
    }

    [Serializable]
    public class Action
    {
        public float duration;
        public List<Input> inputs;
    }

    [Serializable]
    public class Input
    {
        public string name;
        public float value;
    }
    
    [Serializable]
    public class Scenario
    {
        public List<GameObjectValue> startValues;
        public List<Action> actions;
        public float time;
        public string name;
    }
}