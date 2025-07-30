using System;
using System.Collections;
using Cinemachine;
using Events;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Utilities.Events.EventsLayout;

namespace Managers.CameraManager {
    public class ArenaEntryCamera : MonoBehaviour{
        [Header("cameras")]
        [SerializeField] private CinemachineVirtualCamera virtualStaticCamera;
        [SerializeField] private CinemachineVirtualCamera defaultCamera;
        [SerializeField] private float inputOffsetAngle;
        
        [Header("game objects")]
        [SerializeField] private CinemachineBrain unityCamera;
        [SerializeField] private CameraFade cameraFade;

        private float _blendTime;
        
        private SwitchCameraEvent _switchCameraEventChannel;
        private Coroutine _animateCameraRoutine;
        
        private readonly DynamicCameraSettings _dynamicCameraSettings  = new DynamicCameraSettings();
        
        private bool _isUsed = false;
        private void Start() {
            _blendTime = unityCamera.m_DefaultBlend.m_Time;
            _switchCameraEventChannel = (SwitchCameraEvent)EventBroker.TryToAddEventChannel("SwitchCameraEventChannel",
                ScriptableObject.CreateInstance<SwitchCameraEvent>());
            
            _dynamicCameraSettings.Camera = virtualStaticCamera;
            _dynamicCameraSettings.InputOffsetAngle = inputOffsetAngle;
        }

        private void OnTriggerEnter(Collider other) {
            if(_isUsed) return;
            
            if (!other.TryGetComponent<Player.Player>(out var player)) return;
            _isUsed = true;
            _animateCameraRoutine = StartCoroutine(CoCamera());
        }

        private IEnumerator CoCamera() {
            
            unityCamera.m_DefaultBlend.m_Time = 0;
            unityCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
            cameraFade.PrepareFade();
            
            yield return new WaitForSeconds(0.6f);
            
            _switchCameraEventChannel.UpdateValue(_dynamicCameraSettings, null, true);
            
            _dynamicCameraSettings.Camera = defaultCamera;
            
            yield return new WaitForSeconds(0.6f);
            cameraFade.StartFade();
            yield return new WaitForSeconds(3f);
            unityCamera.m_DefaultBlend.m_Time = _blendTime;
            unityCamera.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            _switchCameraEventChannel.UpdateValue(_dynamicCameraSettings,null,false);
        }
    }
}