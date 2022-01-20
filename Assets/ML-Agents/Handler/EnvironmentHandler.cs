using System;
using Unity.MLAgents;
using UnityEngine;

namespace ML_Agents.Handler
{
    public abstract class EnvironmentHandler<T> where T : class, new()
    {
        protected T defaultParameter;
        public T currentParameter = new T();
        public GameObject environment;

        public EnvironmentHandler(GameObject env, T defaultParameter)
        {
            this.environment = env;
            this.defaultParameter = defaultParameter;
        }
        public abstract void ResetParameter();

        public void UpdateEnvironmentParameters()
        {
            var fields = typeof(T).GetFields();


            foreach (var field in fields)
            {
                field.SetValue(currentParameter, Academy.Instance.EnvironmentParameters.GetWithDefault(field.Name,
                    (float)field.GetValue(defaultParameter)));
            }
        }
    }
}