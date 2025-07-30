
using Events.EventsLayout;


namespace Utilities.Events.EventsLayout {
    public class EventWithBool  : BasicEventChannel {
        public bool eventBool = true;

        public void UpdateValue(bool b) {
            eventBool = b;
            RaiseEvent();
        }
    }
}