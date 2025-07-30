using Ami.BroAudio;
using UnityEngine;


namespace PlayerControll.Settings {
    [CreateAssetMenu(menuName = "ScriptableObjects/Stats/Stats Settings")]
    public class StatsSettings : ScriptableObject {
        public float Health = 100f;
        public float Stamina = 100f;
        public int StaminaRecoveryRate = 1;
        public float StaminaRecoveryDelay = 5f;
        public SoundID HurtSound;
        public SoundID DeathSound;
    }
}