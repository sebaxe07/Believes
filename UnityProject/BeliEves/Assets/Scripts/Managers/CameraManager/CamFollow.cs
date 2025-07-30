using System;
using System.Collections.Generic;
using Cinemachine;
using Events;
using Events.EventsLayout;
using UnityEngine;
using Utilities.Events.EventsLayout;
using Object = UnityEngine.Object;

namespace Managers.CameraManager {
    public class CamFollow {
        private CinemachineVirtualCamera _defaultCamera;
        private GameObject _cameraTarget;
        private readonly DynamicCameraSettings _dynamicDefaultCameraSettings = new DynamicCameraSettings();
        
        private Stack<(DynamicCameraSettings cameraSettings, Action CameraExit)> _cameras = new Stack<(DynamicCameraSettings cameraSettings, Action CameraExit)> ();

        private bool _staticCamera = false;
        
        //Events
        private BasicEventChannel _setDefaultCameraEventChannel;
        private BasicEventChannel _popCameraEventChannel;
        private SwitchCameraEvent _switchCameraEventChannel;
        private SwitchCameraEvent _currentCameraEventChannel;
        private EventWithFloat _cameraRotationOffsetEvent;
        
        public CamFollow(GameObject target, CinemachineVirtualCamera defaultCamera) {
            //SetupEvents
            _switchCameraEventChannel =  (SwitchCameraEvent)EventBroker.TryToAddEventChannel("SwitchCameraEventChannel", ScriptableObject.CreateInstance<SwitchCameraEvent>());
            _setDefaultCameraEventChannel = EventBroker.TryToAddEventChannel("SetDefaultCameraEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _setDefaultCameraEventChannel.Subscribe(SetDefaultCamera);
            _popCameraEventChannel = EventBroker.TryToAddEventChannel("PopCameraEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _popCameraEventChannel.Subscribe(PopCamera);
            _switchCameraEventChannel.Subscribe(new Action(()=>SwitchCamera(_switchCameraEventChannel.DynamicCameraSettings, _switchCameraEventChannel.CameraExit, _switchCameraEventChannel.isStatic)));
            _cameraRotationOffsetEvent = (EventWithFloat) EventBroker.TryToAddEventChannel("cameraRotationOffsetEvent", ScriptableObject.CreateInstance<EventWithFloat>());
            _currentCameraEventChannel =  (SwitchCameraEvent)EventBroker.TryToAddEventChannel("CurrentCameraEventChannel", ScriptableObject.CreateInstance<SwitchCameraEvent>());
            
            _defaultCamera = defaultCamera;
            _defaultCamera.enabled = true;
            _defaultCamera.Follow = target.transform;
            
            var defaultCameraAngleEvent = (EventWithFloat)EventBroker.TryToAddEventChannel("defaultCameraAngle", ScriptableObject.CreateInstance<EventWithFloat>());
            _dynamicDefaultCameraSettings.Camera = _defaultCamera;
            _dynamicDefaultCameraSettings.InputOffsetAngle = defaultCameraAngleEvent.value;
            _cameraTarget = target;
            
            _cameras.Push((_dynamicDefaultCameraSettings,null));
        }
        public void SetTarget(GameObject target) {
            _cameraTarget = target;
            if (!_staticCamera) {
                _cameras.Peek().cameraSettings.Camera.Follow = target.transform;
            }
        }

        private void SwitchCamera(DynamicCameraSettings newCameraSettings, Action newCameraExit, bool staticCamera) {
            _staticCamera = staticCamera;
            
            var oldCamera = _cameras.Peek();
            oldCamera.cameraSettings.Camera.gameObject.SetActive(false);
            if(oldCamera.CameraExit!=null) oldCamera.CameraExit();
            _currentCameraEventChannel.UpdateValue(newCameraSettings);
            _cameras.Push((newCameraSettings,newCameraExit));
            _cameraRotationOffsetEvent.UpdateValue(newCameraSettings.InputOffsetAngle);
            if (!_staticCamera) {
                _cameras.Peek().cameraSettings.Camera.Follow = _cameraTarget.transform;
            }
            _cameras.Peek().cameraSettings.Camera.gameObject.SetActive(true);
        }

        private void PopCamera() {
            if(_cameras.Count == 1) return; //avoid remove default camera
            var oldCamera = _cameras.Peek();
            oldCamera.cameraSettings.Camera.gameObject.SetActive(false);
            if(oldCamera.CameraExit!=null) _cameras.Peek().CameraExit();
            _cameras.Pop();
            _cameras.Peek().cameraSettings.Camera.gameObject.SetActive(true);
            _cameras.Peek().cameraSettings.Camera.Follow = _cameraTarget.transform;
            _cameraRotationOffsetEvent.UpdateValue(_cameras.Peek().cameraSettings.InputOffsetAngle);
            _currentCameraEventChannel.UpdateValue(_cameras.Peek().cameraSettings);
        }
        
        private void SetDefaultCamera() {
            var oldCamera = _cameras.Peek();
            oldCamera.cameraSettings.Camera.gameObject.SetActive(false);
            if(oldCamera.CameraExit!=null) _cameras.Peek().CameraExit();
            _defaultCamera.gameObject.SetActive(true);
            _defaultCamera.Follow = _cameraTarget.transform;
            _cameras.Push((_dynamicDefaultCameraSettings, null));
            _cameraRotationOffsetEvent.UpdateValue(_cameras.Peek().cameraSettings.InputOffsetAngle);
            _currentCameraEventChannel.UpdateValue(_cameras.Peek().cameraSettings);
        }
    }
}