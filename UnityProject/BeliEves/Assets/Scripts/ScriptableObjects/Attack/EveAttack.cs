using Ami.BroAudio;
using UnityEngine;

namespace ScriptableObjects.Attack {
    [CreateAssetMenu(menuName = "ScriptableObjects/Attack/Eve Attack")]
    public class EveAttack : ScriptableObject {
        public float dashForceMagnitude = 150f;
        public float dashDistance = 60f;
        public float lightAttackCoolDownTime = 15f;
        public LayerMask dashLayerMask;
        public float specialAttackRate = 0.3f;

        [Header("Attacks Cost")]
        public float lightAttackStaminaCost = 0f;
        public float heavyAttackStaminaCost = 20f;
        public float specialAttackStaminaCost = 100f;

        [Header("Audios")]
        public SoundID lightAttackSound;
        public SoundID Up;
        public SoundID Down;
        public SoundID Flying;
        public SoundID specialAttackSound;
    }
}