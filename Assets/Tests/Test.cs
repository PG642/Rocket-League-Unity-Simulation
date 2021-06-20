using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Tests
{
    [Serializable]

    public class Point
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]

    public class ValuePair
    {
        public string name;
        public Point position;
        public Point velocity;
        public Point rotation;
    }
    [Serializable]

    public class Action
    {
        public string name;
        public Point velocity;
    }

    [Serializable]
    public class Root
    {
        public List<ValuePair> StartValues;
        public List<ValuePair> actions;
        public double time;
    }
}