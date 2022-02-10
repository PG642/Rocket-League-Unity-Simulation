using System;

namespace ML_Agents.Goalkeeper
{
    [Serializable]
    public class GoalkeeperEnvironmentParameters
    {
        public GoalkeeperEnvironmentParameters()
        {
            difficulty = 0;
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
            seed = 0;
        }    
        public float difficulty;
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
        public int seed;
    }
    
}