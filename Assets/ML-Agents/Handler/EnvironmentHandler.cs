namespace ML_Agents.Handler
{
    public abstract class EnvironmentHandler<T> where T : struct
    {
        public T defaultParameter;
        public T currentParameter;
        public abstract void ResetParameter();
        public abstract void UpdateEnvironmentParameters();    
    }
    
}