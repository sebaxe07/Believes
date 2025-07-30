using Events;
using Events.EventsLayout;
using Unity.VisualScripting;
using UnityEngine;

namespace Controller.CameraManager {
    public class CamExit : MonoBehaviour {
        [SerializeField] CamEnter camEnter;
        
        private BasicEventChannel _popCameraEventChannel;

        private void Start() {
            _popCameraEventChannel = EventBroker.TryToAddEventChannel("PopCameraEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
        }
        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player)) return;
            _popCameraEventChannel.RaiseEvent();
            if(camEnter!=null)camEnter.ExitCamera();
        }
        
    }
}