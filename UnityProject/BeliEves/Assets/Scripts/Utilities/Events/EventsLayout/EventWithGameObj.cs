using Events.EventsLayout;
using UnityEngine;


namespace Utilities.Events.EventsLayout
{
    public class EventWithGameObj: BasicEventChannel {
        public GameObject gameObject;
        public void UpdateValue(GameObject gameObject) {
            this.gameObject = gameObject;
            RaiseEvent();
        }
    }
}