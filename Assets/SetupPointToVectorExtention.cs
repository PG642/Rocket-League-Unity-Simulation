using JsonObjects;
using UnityEngine;

namespace DefaultNamespace
{
    public static class SetupPointToVectorExtention
    {
        public static Vector3 ToVector(this SetupPoint point, float offsetX=0.0f,float offsetY=0.0f,float offsetZ=0.0f)
        {
            return new Vector3(point.x + offsetX, point.y + offsetY, point.z + offsetZ);
        }
        
        public static Quaternion ToQuaternion(this SetupPoint point)
        {
            return Quaternion.Euler(point.x, point.y, point.z);
        }
    }
}