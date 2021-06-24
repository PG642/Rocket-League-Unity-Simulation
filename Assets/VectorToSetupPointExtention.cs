
using JsonObjects;
using UnityEngine;

namespace DefaultNamespace
{
    public static class VectorToSetupPointExtention
    {
        public static SetupPoint ToVector(this Vector3 point)
        {
            return new SetupPoint()
            {
                x = point.x, 
                y = point.y, 
                z = point.z
            };
        }
    }
}