using System.Security.Cryptography.X509Certificates;
using Events.EventsLayout;
using UnityEngine;

namespace Utilities.Events.EventsLayout {
    public class TeleportEvent: BasicEventChannel {
        public Vector3 destination;
        public Quaternion destinationRotation;

        public void UpdateValue(Vector3 destination, Quaternion destinationRotation) {
            this.destination = destination;
            this.destinationRotation = destinationRotation;
            
            RaiseEvent();
        }
    }
}