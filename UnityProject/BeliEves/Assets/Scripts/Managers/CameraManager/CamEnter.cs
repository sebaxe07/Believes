using System;
using Cinemachine;
using Events;
using Events.EventsLayout;
using Managers.CameraManager;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Controller.CameraManager {
    public class CamEnter : MonoBehaviour {
        [SerializeField] private CinemachineVirtualCamera setCamera;
        [SerializeField] private bool disableOnExit = false;
        [SerializeField] private float inputOffsetAngle;
        [SerializeField] private bool _activated;
        
        
        private SwitchCameraEvent _switchCameraEventChannel;
        private BasicEventChannel _popCameraEventChannel;
        
        private readonly DynamicCameraSettings _dynamicCameraSettings  = new DynamicCameraSettings();
        private readonly object _lockObject = new object();
        private void Start() {
            _switchCameraEventChannel = (SwitchCameraEvent)EventBroker.TryToAddEventChannel("SwitchCameraEventChannel",
                ScriptableObject.CreateInstance<SwitchCameraEvent>());
            _popCameraEventChannel = EventBroker.TryToAddEventChannel("PopCameraEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());

            _dynamicCameraSettings.Camera = setCamera;
            _dynamicCameraSettings.InputOffsetAngle = inputOffsetAngle;
        }

        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player)) return;
            lock (_lockObject) {
                if (!_activated) {
                    //_cameraRotationOffsetEvent.UpdateValue(inputOffsetAngle);
                    _activated = true;
                    _switchCameraEventChannel.UpdateValue(_dynamicCameraSettings, ExitCamera, false);
                }
                else if (!disableOnExit) {
                    _activated = false;
                    _popCameraEventChannel.RaiseEvent();
                }
            }
        }


        public void ExitCamera() {
            lock (_lockObject) {
                _activated = false;
            }
        }

        public String GetCameraName() {
            return setCamera.name;
        }

        public void SetActive() {
            lock (_lockObject) {
                _activated = true;
            }
        }
        
        private void OnTriggerExit(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player)) return;
            if(!_activated || !disableOnExit) return;
                
            _activated = false;
            _popCameraEventChannel.RaiseEvent();
        }
    }
}