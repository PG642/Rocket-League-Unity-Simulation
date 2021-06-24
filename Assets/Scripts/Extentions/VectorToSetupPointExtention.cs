using TestScenarios.JsonClasses;
using UnityEngine;

namespace Extentions
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
        
        public static SetupPoint ToVector(this Quaternion quaternion)
        {
            var angles = quaternion.eulerAngles;

            return new SetupPoint()
            {
                x = angles.x,
                y = angles.y, 
                z = angles.z
            };
        }
    }
}