using UnityEngine;

namespace Utilities.Events.EventsLayout
{
    public class EventTakeDamage : EventWithGameObj
    {
        public float damageAmount = 0;

        public void UpdateValue(GameObject gameObject, float damageAmount)
        {
            this.damageAmount = damageAmount;
            base.UpdateValue(gameObject);
        }
    }
}