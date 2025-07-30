using Ami.BroAudio;
using UnityEngine;

namespace ScriptableObjects.Attack {
    [CreateAssetMenu(menuName = "ScriptableObjects/Attack/Tank Attack")]
    public class TankAttack : ScriptableObject {

        [Header("Player Settings")]
        [Header("Light Attack Settings")]
        public float lightAttackDamage = 10f;
        public float lightAttackCooldown = 1f;
        public float lightAttackDuration = 0.5f;
        public float lightAttackStaminaCost = 10f;
        [Header("Heavy Attack Settings")]
        public float heavyAttackForce = 50f;
        public float heavyAttackDamage = 20f;
        public float heavyAttackYForce = 1f;
        public float heavyAttackCooldown = 5f;
        public float heavyAttackDuration = 0.5f;
        public float heavyAttackStaminaCost = 20f;
        [Header("Special Attack Settings")]
        public float specialAttackDamage = 10f;
        public float specialAttackDuration = 5f;
        public float specialAttackCooldown = 5f;
        public float specialAttackStaminaCost = 100f;

        [Header("Enemy Settings")]
        public float lightAttackDamageEnemy = 20f;
        public float heavyAttackForceEnemy = 50f;
        public float heavyAttackDamageEnemy = 20f;
        public float heavyAttackCooldownEnemy = 5f;
        public float lightAttackCooldownEnemy = 1f;
        public float specialAttackCooldownEnemy = 1f;
        public float specialAttackDamageEnemy = 10f;

        [Header("Audio Settings")]
        public SoundID lightPrepareAttackSound;
        public SoundID lightAttackSound;
        public SoundID heavyAttackSound;
        public SoundID specialAttackSound;
        public SoundID jumpSound;
        public SoundID steps;

    }
}