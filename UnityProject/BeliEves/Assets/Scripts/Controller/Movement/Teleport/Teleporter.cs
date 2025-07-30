using Cinemachine;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Controller.Movement.Teleport {
    public class Teleporter : MonoBehaviour {
        [SerializeField] private TeleportDestination destination;
        [SerializeField] private Vector3 destinationOffset;
        [SerializeField] private Quaternion destinationRotation;
        [SerializeField] private bool useCurrentRotation;
        
        [Header("don't touch")]
        public bool isTeleporting = false;
        
        private TeleportEvent _teleportEvent;
        private Vector3 _destination;
        
        private CinemachineBrain brain;

        private void Start() {
            _destination = destination.transform.position + destinationOffset;
            
            _teleportEvent = (TeleportEvent)EventBroker.TryToAddEventChannel("teleportEvent",ScriptableObject.CreateInstance<TeleportEvent>());
            
            brain = Camera.main.GetComponent<CinemachineBrain>();
            destination.SetStart(this);
        }

        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player) || isTeleporting) return;
            
            CinemachineVirtualCamera activeVirtualCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            
            var rotation = destinationRotation;
            if(useCurrentRotation)rotation = other.transform.rotation;
            destination.isTeleporting = true;
            
            var deltaPosition = _destination - player.transform.position;
            activeVirtualCamera?.OnTargetObjectWarped(player.gameObject.transform, deltaPosition);
            _teleportEvent.UpdateValue(_destination, rotation);
        }
        
        private void OnTriggerExit(Collider other) {
            isTeleporting = false;
        }
    }
}