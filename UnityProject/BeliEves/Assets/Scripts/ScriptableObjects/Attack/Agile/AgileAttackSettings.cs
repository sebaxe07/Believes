using Ami.BroAudio;
using Controller.Attack;
using UnityEngine;

namespace ScriptableObjects.Attack {
    [CreateAssetMenu(menuName = "ScriptableObjects/Attack/Agile Attack")]
    public class AgileAttackSettings : ScriptableObject {

        [Header("Attack settings")]
        [Tooltip("all the settings related to the attack system")]
        public float punchDamageAmount;
        public float grapplingDamageAmount;
        public float grapplingMaxDistance;
        public float grapplingSpinDamageAmount;
        public float grapplingSpinRadius;
        public float grapplingSpinSpeed;
        public int grapplingSpinRotationTime;

        [Header("Enemy settings")]
        public float punchDamageAmountEnemy;
        public float grapplingShootingDamage;
        public float grapplingShootingSpeed;
        public float grapplingShootingCooldown;

        [Header("grappling settings")]
        [Tooltip("all the settings related to the grappling")]
        public LayerMask whatIsGrappable;
        public int quality;
        public float damper;
        public float strength;
        public float velocity;
        public float waveCount;
        public float waveHeight;
        public AnimationCurve affectCurve;

        [Header("Draggable settings")]
        public float draggableSpeed;
        public float draggableMaxDistance;
        public float draggableSafeOffset;

        [Header("GrapplingAnchorPoint settings")]
        public float anchorTime;
        public float anchorMaxDistance;

        [Header("Attacks Cost")]
        public float lightAttackStaminaCost = 0f;
        public float heavyAttackStaminaCost = 20f;
        public float specialAttackStaminaCost = 100f;

        [Header("Attacks Timer")]
        public float lightAttackDuration = 0.5f;
        public float specialAttackCooldown = 5f;
        public float specialAttackDuration = 1f;
        public float heavyAttackCooldown = 0.5f;
        public float lightAttackCooldown = 1f;

        [Header("Enemy Cooldowns")]
        public float lightAttackCooldownEnemy = 1f;
        public float heavyAttackCooldownEnemy = 1f;

        [Header("Audio Settings")]
        public SoundID lightPrepareAttackSound;
        public SoundID lightAttackSound;
        public SoundID specialAttackSound;
        public SoundID jumpSound;
        public SoundID grapplingShootSound;
        public SoundID grapplingReelSound;
        public SoundID grapplingHitSound;
        public SoundID steps;


    }
}