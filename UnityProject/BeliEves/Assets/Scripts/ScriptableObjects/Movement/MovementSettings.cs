using UnityEngine;


namespace PlayerControll.Settings {
    [CreateAssetMenu (menuName="ScriptableObjects/Movement/movement Settings")]
    public class MovementSettings: ScriptableObject{
        public float WalkSpeed = 10f;
        public float JumpSpeed = 10f;

        public float Mass = 5f;
        public float onGroundMaxDistance = 0.55f;
        public float SmoothFactor = 0.05f;
    }
}