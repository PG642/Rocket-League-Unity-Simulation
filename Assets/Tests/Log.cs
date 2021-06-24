using System;
using System.Collections.Generic;

namespace JsonObjects
{
    [Serializable]
    public class Log
    {
        public List<LogValue> logValues;
    }

    [Serializable]
    public class LogValue
    {
        public float time;
        public GameObjectValue gameObjectValue;
    }
}