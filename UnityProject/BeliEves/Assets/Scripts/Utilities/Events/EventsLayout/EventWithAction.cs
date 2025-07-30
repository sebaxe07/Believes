using System;
using Events.EventsLayout;

namespace Utilities.Events.EventsLayout {
    public class EventWithAction : BasicEventChannel {
        public Action EventAction;

        public void UpdateValue(Action action) {
            EventAction = action;
            RaiseEvent();
        }
    }
}