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
        public List<GameCar> game_cars;
        public BallValues game_ball;
    }

    [Serializable]
    public class GameObjectSaveValue
    {
        public SetupPoint location;
        public SetupPoint velocity;
        public SetupPoint angular_velocity;
        public SetupPoint rotation;
        
    }

    
    [Serializable]
    public class BallValues
    {
        public GameObjectSaveValue physics;
    }
    
    [Serializable]
    public class GameCar
    {
        public GameObjectSaveValue physics;
        public bool has_wheel_contact;
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