using System;
using Cinemachine;
using Events.EventsLayout;
using Managers.CameraManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities.Events.EventsLayout {
    public class SwitchCameraEvent: BasicEventChannel {
        //public CinemachineVirtualCamera vCamera;
        public Action CameraExit;
        public bool isStatic = false;
        public DynamicCameraSettings DynamicCameraSettings;
        //public float inputOffsetAngle;
        
        public void UpdateValue(DynamicCameraSettings dynamicCameraSettings, Action cameraExit = null, bool IsStatic = false) {
            this.CameraExit = cameraExit;
            this.isStatic = IsStatic;
            this.DynamicCameraSettings = dynamicCameraSettings;
            RaiseEvent();
        }
    }
}