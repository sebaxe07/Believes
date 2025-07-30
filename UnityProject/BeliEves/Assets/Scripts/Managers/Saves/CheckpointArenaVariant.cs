using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Interactable;

namespace Managers.Saves {
    public class CheckpointArenaVariant : Interactable {
        private GameManager _gameManager;
        
        private bool _isActive = false;
        private GameObject _lightGlow;
        //playerInput
        private PlayerInput _playerInput;
        private InputAction _interactionAction;
        public void Awake() {
            _gameManager = FindObjectOfType<GameManager>();
            _lightGlow = transform.Find("LightGlow").gameObject;
            
            //initializePlayerInput
            _playerInput = FindObjectOfType<PlayerInput>();
            _interactionAction = _playerInput.actions["Interaction"];
        }
        public override void Interact() {
            if(_isActive)return;
            
            _lightGlow.SetActive(true);
            _isActive = true;
            _gameManager.UpdateArenaRespawnData(this, new Action(() => {
                interactable.ShowSimpleHelpText("saving done!");
            }));
        }

        protected override void OnTriggerEnterCallback() {
            if (_isActive) {
                interactable.ShowSimpleHelpText("already saved!");
                return;
            }
            
            
            interactable.ShowHelpText("Press ", " to Save the game");
            _lightGlow.SetActive(true);
        }

        protected override void OnTriggerExitCallback() {
            interactable.HideHelpText();
            if(!_isActive)_lightGlow.SetActive(false);
        }
        
        public void Toggle(bool active) {
            _isActive = active;
            if(!_isActive)_lightGlow.SetActive(false);
        }
    }
}