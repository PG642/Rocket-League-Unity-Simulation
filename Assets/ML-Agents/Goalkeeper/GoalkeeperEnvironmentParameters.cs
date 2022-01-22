using System;

namespace ML_Agents.Goalkeeper
{
    [Serializable]
    public class GoalkeeperEnvironmentParameters
    {
        
        public float difficulty;
        public float canDoubleJump;
        public float useSuspension;
        public float initialBoost;
        public float canBoost;    
        public float useBulletImpulse;
        public float usePsyonixImpulse;
        public float useCustomBounce;
        public float useWallStabilization;
        public float useGroundStabilization;
        public float canDrift;
    }
}