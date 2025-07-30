using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class EventHeal : EventWithGameObj {
        public float healRate = 0;
        public float healDuration = 0;

        public void UpdateValue(GameObject gameObject, float healRate, float healDuration) {
            this.healRate = healRate;
            this.healDuration = healDuration;
            base.UpdateValue(gameObject);
        }
    }
}