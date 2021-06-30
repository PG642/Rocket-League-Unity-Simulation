using TestScenarios.JsonClasses;
using UnityEngine;

namespace Extentions
{
    public static class VectorToSetupPointExtention
    {
        public static SetupPoint ToVector(this Vector3 point, float offsetY=0.0f)
        {
            return new SetupPoint()
            {
                x = point.x, 
                y = point.y + offsetY, 
                z = point.z
            };
        }
        
        public static Rotation ToVector(this Quaternion quaternion)
        {
            var angles = quaternion.eulerAngles;

            return new Rotation()
            {
                pitch = angles.x,
                yaw = angles.y, 
                roll = angles.z
            };
        }
    }
}