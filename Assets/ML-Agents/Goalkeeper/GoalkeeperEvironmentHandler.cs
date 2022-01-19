using System;
using ML_Agents.Handler;
using Unity.MLAgents;

namespace ML_Agents.Goalkeeper
{
    [Serializable]
    public class GoalkeeperEvironmentHandler
    {
        public GoalkeeperEnvironmentParameters currentParameter;
        private GoalkeeperEnvironmentParameters _defaultParameter;
        public GoalkeeperEvironmentHandler(GoalkeeperEnvironmentParameters defaultParameter)
        {
            _defaultParameter = defaultParameter;
        }
        public void ResetParameter()
        {
            UpdateEnvironmentParameters();
            //TODO ändere Environment
            
        }

        public void UpdateEnvironmentParameters()
        {
            currentParameter.difficulty =
                (int)Academy.Instance.EnvironmentParameters.GetWithDefault("difficulty", _defaultParameter.difficulty);

        }
    }
}