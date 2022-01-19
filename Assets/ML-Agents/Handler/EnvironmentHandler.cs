using System;
using Unity.MLAgents;

namespace ML_Agents.Handler
{
    public abstract class EnvironmentHandler<T> where T : class, new()
    {
        protected T _defaultParameter;
        public T currentParameter = new T();
        public abstract void ResetParameter();

        public void UpdateEnvironmentParameters()
        {
            var fields = typeof(T).GetFields();


            foreach (var field in fields)
            {
                field.SetValue(currentParameter, Academy.Instance.EnvironmentParameters.GetWithDefault(field.Name,
                    (float)field.GetValue(_defaultParameter)));
            }
        }
    }
}