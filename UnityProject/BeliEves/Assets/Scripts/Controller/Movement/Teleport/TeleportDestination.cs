using System;
using Cinemachine;
using Events;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Controller.Movement.Teleport {
    public class TeleportDestination : MonoBehaviour {
        [SerializeField] private bool allowReversTeleport;
        
        
        [Header("don't touch")]
        public bool isTeleporting = false;
        
        private Vector3 _startPosition;
        private Teleporter _teleporter;
        private TeleportEvent _teleportEvent;
        private CinemachineBrain brain;
        private void Start() {
            _teleportEvent = (TeleportEvent)EventBroker.TryToAddEventChannel("teleportEvent",ScriptableObject.CreateInstance<TeleportEvent>());
            brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        public void SetStart(Teleporter teleporter) {
            _teleporter = teleporter;
            _startPosition = teleporter.transform.position;
        }
        
        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player) || !allowReversTeleport || isTeleporting) return;
            
            CinemachineVirtualCamera activeVirtualCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;

            
            var rotation = other.transform.rotation;
            _teleporter.isTeleporting = true;
           
            var deltaPosition = _startPosition - player.transform.position;
            activeVirtualCamera?.OnTargetObjectWarped(player.gameObject.transform, deltaPosition);
            _teleportEvent.UpdateValue(_startPosition, rotation);
        }

        private void OnTriggerExit(Collider other) {
            isTeleporting = false;
        }
    }
}