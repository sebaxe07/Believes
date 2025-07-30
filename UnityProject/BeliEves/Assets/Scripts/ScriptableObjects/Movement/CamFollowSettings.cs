using UnityEngine;

namespace Settings.Movement
{
    [CreateAssetMenu (menuName="ScriptableObjects/Movement/Cam Follow Settings")]
    public class CamFollowSettings: ScriptableObject{
        [Header("Camera settings")]
        [Tooltip("change how the camera follows the target")]
        public float SmoothTime = 1f;
        public Vector3 offset;
    }
}