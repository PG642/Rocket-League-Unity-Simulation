using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace TestScenarios.JsonClasses
{
    [Serializable]
    public class Log
    {
        public List<LogValue> frames;
        public string name;
    }

    [Serializable]
    public class LogValue
    {
        public float time;
        public GameCar carValue;
        public GameObjectSaveValue ballValue;
    }

    [Serializable]
    public class GameObjectSaveValue
    {
        public SetupPoint position;
        public SetupPoint velocity;
        public SetupPoint angularVelocity;
        public Rotation rotation;
        
    }

    [Serializable]
    public class BallValues : GameObjectSaveValue
    {
        
    }
    
    [Serializable]
    public class GameCar: GameObjectSaveValue
    {
        public bool hasWheelContact;
        public bool jumped;
        public float boost;
    }

    [Serializable]
    public class Rotation
    {
        public float pitch;
        public float roll;
        public float yaw;
    }
    
        
    
    
}