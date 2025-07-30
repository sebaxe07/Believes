using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities.Interactable {
    public class InteractableGameObject : MonoBehaviour {
        [SerializeField] private Interactable interactable;
        [SerializeField] private GameObject customText;
        [SerializeField] private GameObject customImage;

        //playerInput
        private PlayerInput _playerInput;
        private InputAction _interactionAction;

        private GameObject _userHelpText;
        private GameObject _defaultHelpText;
        private GameObject _userHelpTextImage;
        private string _interactionActionName;

        public void Awake() {
            _playerInput = FindObjectOfType<PlayerInput>();
            _interactionAction = _playerInput.actions["Interaction"];

            var bottomGameUI = GameObject.Find("BottomGameUI");
            _userHelpText = customText != null ? customText : GetChildByName(bottomGameUI,"UserHelpText");
            _userHelpTextImage = customImage != null ? customImage : GetChildByName(bottomGameUI,"BackgroundHelperTextImage");
            _defaultHelpText = GetChildByName(bottomGameUI,"UserHelpText");
            
            
            var control = InputSystem.FindControl(_interactionAction.bindings[0].effectivePath);
            _interactionActionName = control.name.ToUpper();

            this.enabled = false;
        }

        private GameObject GetChildByName(GameObject parent, string childName) {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == childName)
                    return child.gameObject;
            }
            return null;
        }
        
        public void ShowHelpText(string text1 = "", string text2 = "") {
            if (_userHelpText == null) return;
            
            if(_userHelpText!=_defaultHelpText)_defaultHelpText.SetActive(false);
            _userHelpTextImage.SetActive(true);
            
            _userHelpText.SetActive(true);
            _userHelpText.GetComponent<TextMeshProUGUI>().text = text1  + "[" + _interactionActionName + "]" + text2;
        }

        public void ShowSimpleHelpText(string text1 = "") {
            if (_userHelpText == null) return;
            
            _userHelpTextImage.SetActive(true);
            
            _userHelpText.SetActive(true);
            _userHelpText.GetComponent<TextMeshProUGUI>().text = text1;
        }

        public void HideHelpText() {
            _userHelpTextImage.SetActive(false);
            _userHelpText.SetActive(false);
        }

        private void Update() {
            if (this.interactable != null && _interactionAction.WasPressedThisFrame()) {
                this.interactable.Interact();
            }
        }
    }
}