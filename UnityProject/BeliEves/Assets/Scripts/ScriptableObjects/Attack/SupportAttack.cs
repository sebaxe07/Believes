using Ami.BroAudio;
using UnityEngine;

namespace ScriptableObjects.Attack {
    [CreateAssetMenu(menuName = "ScriptableObjects/Attack/Support Attack")]
    public class SupportAttack : ScriptableObject {

        [Header("Player Settings")]
        [Header("Light Attack Settings")]
        public float lightAttackDamage = 10f;
        public float lightAttackCooldown = 1f;
        public float lightAttackDuration = 0.5f;
        public float lightAttackStaminaCost = 10f;
        [Header("Heavy Attack Settings")]
        public float heavyAttackDamage = 10f;
        public float heavyAttackStunDuration = 0.5f;
        public float heavyAttackCooldown = 5f;
        public float heavyAttackDuration = 0.5f;
        public float heavyAttackStaminaCost = 20f;
        [Header("Special Attack Settings")]
        public float specialAttackHealRate = 10f;
        public float specialAttackRate = 5f;
        public float specialAttackCooldown = 5f;
        public float specialAttackStaminaCost = 100f;

        [Header("Enemy Settings")]
        public float lightAttackDamageEnemy = 20f;
        public float heavyAttackDamageEnemy = 20f;
        public float lightAttackCooldownEnemy = 1f;
        public float heavyAttackStunEnemy = 0.5f;
        public float heavyAttackCooldownEnemy = 5f;


        [Header("Audio Settings")]

        public SoundID lightPrepareAttackSound;
        public SoundID lightAttackSound;
        public SoundID heavyAttackSound;
        public SoundID specialAttackSound;
        public SoundID jumpSound;
        public SoundID steps;
    }
}