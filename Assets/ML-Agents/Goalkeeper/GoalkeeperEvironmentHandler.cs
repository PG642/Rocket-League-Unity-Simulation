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
            if (currentParameter.canDoubleJump == 0)
            {
                
            }
            if (currentParameter.canBoost == 0)
            {

            }
            if (currentParameter.canDrift == 0)
            {

            }
            if (currentParameter.useBulletImpulse == 0)
            {

            }
            if (currentParameter.useCustomBounce == 0)
            {

            }
            if (currentParameter.useGroundStabilization == 0)
            {

            }
            if (currentParameter.usePsyonixImpulse == 0)
            {

            }
            if (currentParameter.useSuspension == 0)
            {

            }
            if (currentParameter.useWallStabilization == 0)
            {

            }

        }


    }
}