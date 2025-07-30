using Events;
using Events.EventsLayout;
using UnityEngine;

namespace Player {
    public class SupportHealthInput : RobotAttackInput {
        [SerializeField]private float minTimeForLong = 0.5f;
        
        private BasicEventChannel _stopHealingEvent;
        private float _startingTime = 0;
        public void Awake() {
            _stopHealingEvent = EventBroker.TryToAddEventChannel("stopHealingEvent", ScriptableObject.CreateInstance<BasicEventChannel>());
        }

        private new void Update() {
            base.Update();
            
            if (SpecialAttackAction.WasPressedThisFrame()) _startingTime = Time.time;
            if (SpecialAttackAction.WasReleasedThisFrame() && Time.time - _startingTime >= minTimeForLong) {
                _startingTime = 0f; // Reset the starting time.
                _stopHealingEvent.RaiseEvent(); // Trigger the action.
                _stopHealingEvent.RaiseEvent(); // Trigger the action.
            }
        }
    }
}