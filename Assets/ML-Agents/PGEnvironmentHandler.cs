using UnityEngine;
using Unity.MLAgents;
using ML_Agents.Handler;
using System;

namespace PGAgent.ResetParameters
{

    public class PGEnvironmentHandler<T> : EnvironmentHandler<T> where T : PGResetParameters, new()
    {
        private ResetParameterHandler<T> _handler;
        public PGEnvironmentHandler(GameObject env, T defaultParameter) : base(env, defaultParameter)
        {
            this._handler = null;
        }

        public PGEnvironmentHandler(GameObject env, T defaultParameter, ResetParameterHandler<T> handler) : base(env, defaultParameter)
        {
            this._handler = handler;
        }

        public override void ResetParameter()
        {
            UpdateEnvironmentParameters();
            // Determine what seed to use for this episode, if the seed is set to a negative number use a new random seed for each episode
            if (currentParameter.seed >= 0)
                UnityEngine.Random.InitState((int)currentParameter.seed);
            else
            {
                System.Random rand = new System.Random();
                UnityEngine.Random.InitState(rand.Next(1000));
            }
            //TODO after merge with difficulty, add difficulty parameter
            if (currentParameter.canDoubleJump == 0)
            {
                environment.GetComponentInChildren<CubeJumping>().disableDoubleJump = true;
            }
            if (currentParameter.canBoost == 0)
            {
                environment.GetComponentInChildren<CubeBoosting>().disableBoosting = true;
            }
            else
            {
                environment.GetComponentInChildren<CubeBoosting>().boostAmount = currentParameter.initialBoost;
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
            if (_handler != null)
            {
                _handler.Handle(currentParameter, environment);
            }
        }
    }

    public interface ResetParameterHandler<T> where T : PGResetParameters, new()
    {
        void Handle(T parameters, GameObject environment);
    }

    [Serializable]
    public class PGResetParameters
    {
        public PGResetParameters()
        {
            seed = -1;
            initialBoost = 32;
            canBoost = 1;
            canDoubleJump = 1;
            canDrift = 1;
            canDoubleJump = 1;
            useBulletImpulse = 1;
            usePsyonixImpulse = 1;
            useSuspension = 1;
            useCustomBounce = 1;
            useWallStabilization = 1;
            useGroundStabilization = 1;
        }
        public float seed;
        public float initialBoost;
        public float canDoubleJump;
        public float canDrift;
        public float canBoost;
        public float useSuspension;
        public float useBulletImpulse;
        public float usePsyonixImpulse;
        public float useCustomBounce;
        public float useWallStabilization;
        public float useGroundStabilization;
    }
}