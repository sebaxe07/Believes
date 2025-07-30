using System;
using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class EventUseStamina : EventWithGameObj {
        public float staminaAmount = 0;
        public Action<Exception> OnStaminaUsed;
        public bool ToggleConsumption = false;
        public float rateConsumption = 1f;
        public void UpdateValue(GameObject gameObject, float staminaAmount, Action<Exception> callback, bool ToggleConsumption = false, float rateConsumption = 1f) {
            this.staminaAmount = staminaAmount;
            this.OnStaminaUsed = callback;
            this.ToggleConsumption = ToggleConsumption;
            this.rateConsumption = rateConsumption;
            base.UpdateValue(gameObject);
        }
    }
}