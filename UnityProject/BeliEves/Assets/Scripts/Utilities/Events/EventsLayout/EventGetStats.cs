using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class EventGetStats : EventWithGameObj {
        public float currentHealth = 0;
        public float currentStamina = 0;

        public bool b;

        public void UpdateValue(GameObject gameObject, float currentHealth, float currentStamina, bool b = false) {
            this.currentHealth = currentHealth;
            this.currentStamina = currentStamina;
            this.b = b;
            base.UpdateValue(gameObject);
        }
    }
}