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
        public string id;
        public bool lead;
        public string team;
        public StartValues startValues;
        public List<Action> actions;
        public int boost;
    }

    [Serializable]
    public class StartValues
    {
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
        public List<GameObjectValue> gameObjects;
        public float time;
        public string name;
    }
}