using UnityEngine;

namespace JHT.Scripts.Common.PerformanceExtension
{
    public static class VectorExtension
    {
        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max) => new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
    }
}