using Events.EventsLayout;

namespace Utilities.Events.EventsLayout {
    public class EventWithFloat : BasicEventChannel {
        public float value;
        
        public void UpdateValue(float value) {
            this.value = value;
            RaiseEvent();
        }
    }
}