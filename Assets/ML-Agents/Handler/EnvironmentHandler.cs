namespace ML_Agents.Handler
{
    public abstract class EnvironmentHandler<T> where T : struct
    {
        protected T _defaultParameter;
        public T currentParameter;
        public abstract void ResetParameter();
        public abstract void UpdateEnvironmentParameters();    
    }
    
}