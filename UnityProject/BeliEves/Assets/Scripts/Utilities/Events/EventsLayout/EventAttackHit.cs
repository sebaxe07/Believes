using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class EventAttackHit : EventWithGameObj {
        public GameObject hitObj = null;
        public void UpdateValue(GameObject gameObject, GameObject hitObj) {
            this.hitObj = hitObj;
            base.UpdateValue(gameObject);
        }
    }
}
