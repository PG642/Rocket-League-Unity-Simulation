using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace JsonObjects
{
    [Serializable]
    public class Point
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class StartValue
    {
        public string gameObject;
        public Point position;
        public Point velocity;
        public Point angularVelocity;
        public Point rotation;
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
    public class RootJson
    {
        public List<Scenario> scenarios;
    }
    [Serializable]
    public class Scenario
    {
        public List<StartValue> startValues;
        public List<Action> actions;
        public float time;
        public string name;
    }
}