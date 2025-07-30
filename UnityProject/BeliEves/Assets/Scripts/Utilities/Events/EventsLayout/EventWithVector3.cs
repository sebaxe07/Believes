using Events.EventsLayout;
using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class EventWithVector3 : BasicEventChannel {
        public Vector3 vector;

        public void UpdateValue(Vector3 vector) {
            this.vector = vector;
            RaiseEvent();
        }
    }
}