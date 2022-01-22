using System;
using ML_Agents.Handler;
using Unity.MLAgents;
using UnityEngine;

namespace ML_Agents.Goalkeeper
{
    [Serializable]
    public class GoalkeeperEvironmentHandler: EnvironmentHandler<GoalkeeperEnvironmentParameters>
    {
        public GoalkeeperEvironmentHandler(GameObject env, GoalkeeperEnvironmentParameters defaultParameter) : base(env, defaultParameter)
        {
        }

        public override void ResetParameter()
        {
            UpdateEnvironmentParameters();
            if (currentParameter.canDoubleJump == 0)
            {
                environment.GetComponentInChildren<CubeJumping>().disableDoubleJump = true;
            }
            if (currentParameter.canBoost == 0)
            {
                environment.GetComponentInChildren<CubeBoosting>().disableBoosting = true;
            }
            if (currentParameter.canDrift == 0)
            {
                environment.GetComponentInChildren<CubeGroundControl>().disableDrift = true;
            }
            if (currentParameter.useBulletImpulse == 0)
            {
                environment.GetComponentInChildren<Ball>().disableBulletImpulse = true;
            }
            if (currentParameter.useCustomBounce == 0)
            {
                environment.GetComponentInChildren<Ball>().disableCustomBounce = true;
            }
            if (currentParameter.useGroundStabilization == 0)
            {
                environment.GetComponentInChildren<CubeGroundControl>().disableGroundStabilization = true;
            }
            if (currentParameter.usePsyonixImpulse == 0)
            {
                environment.GetComponentInChildren<Ball>().disablePsyonixImpulse = true;
            }
            if (currentParameter.useSuspension == 0)
            {
                SuspensionCollider[] suspensions = environment.GetComponentsInChildren<SuspensionCollider>();
                foreach (SuspensionCollider suspension in suspensions)
                {
                    suspension.disableSuspension = true;
                }
            }
            if (currentParameter.useWallStabilization == 0)
            {
                environment.GetComponentInChildren<CubeGroundControl>().disableWallStabilization = true;
            }

        }


    }
}