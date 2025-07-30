using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Movement {
    [CreateAssetMenu (menuName="ScriptableObjects/Movement/movement Kinematic Settings")]
    public class KinematicMovementSettings: ScriptableObject{
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public float RunSpeedUpFactor = 3f;
        
        [Header("Misc")]
        public List<GameObject> IgnoredColliders = new List<GameObject>();
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public float BonusOrientationSharpness = 10f;
        
        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;
        
        [Header("Jumping")]
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPostGroundingGraceTime = 0f;
        public float JumpPreGroundingGraceTime = 0f;
        
        [Header("Jumping")]
        public float MaxStepHeight = 1f;
        public float MinRequireStepDepth = 0f;
    }
}