using System;
using ML_Agents.Handler;
using Unity.MLAgents;

namespace ML_Agents.Goalkeeper
{
    [Serializable]
    public class GoalkeeperEvironmentHandler: EnvironmentHandler<GoalkeeperEnvironmentParameters>
    {

        public GoalkeeperEvironmentHandler(GoalkeeperEnvironmentParameters defaultParameter)
        {
            _defaultParameter = defaultParameter;
        }
        public override void ResetParameter()
        {
            UpdateEnvironmentParameters();
            //TODO ändere Environment
            if (currentParameter.doubleJump)
            {
                
            }
            
        }

        public override void UpdateEnvironmentParameters()
        {
            currentParameter.difficulty =
                (int)Academy.Instance.EnvironmentParameters.GetWithDefault("difficulty", _defaultParameter.difficulty);

        }
    }
}