using System;
using System.ComponentModel;
using System.Reflection;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Events.EventsLayout;

namespace Managers.DialogueManager {
    //playerInput

    public enum InputType{
        Interaction,
        Jump,
        Run,
        LightAttack,
        HeavyAttack,
        SpecialAttack,
        EveRelease
    }

    public enum ActionType {
        [Description("press")]
        Press,
        [Description("keep press")]
        KeepPress,
    }

    public class HelperCollider : MonoBehaviour {
        [SerializeField] private InputType inputName;
        [SerializeField] private ActionType actionType;
        [SerializeField] private String actionComment;
        [SerializeField] private bool blockingTutorial;
        
        private PlayerInput _playerInput;
        private InputAction _interactionAction;
        private GameObject _userHelpText;
        private GameObject _userHelpTextImage;
        private string _interactionActionName;
        private bool _interacting;
        
        //Events
        private EventWithBool _movementEnabledEvent;
        
        private void Awake() {
            _playerInput = FindObjectOfType<PlayerInput>();
            _interactionAction = _playerInput.actions[inputName.ToString()];
            
            var bottomGameUI = GameObject.Find("BottomGameUI");
            _userHelpText = GetChildByName(bottomGameUI,"UserHelpText");
            _userHelpTextImage = GetChildByName(bottomGameUI,"BackgroundHelperTextImage");

            _movementEnabledEvent = (EventWithBool)EventBroker.TryToAddEventChannel("movementEnableEvent", ScriptableObject.CreateInstance<EventWithBool>());
            
            var control = InputSystem.FindControl(_interactionAction.bindings[0].effectivePath);
            _interactionActionName = control.name.ToUpper();
        }

        private void Update() {
            if (_interacting && _interactionAction.WasPressedThisFrame()) TutorialButtonPressed();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<Player.Player>(out var player)) {
                if(blockingTutorial)_movementEnabledEvent.UpdateValue(false);
                _interacting = true;
                ShowHelpText(GetEnumDescription(actionType), actionComment);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.TryGetComponent<Player.Player>(out var player)) {
                if(blockingTutorial)_movementEnabledEvent.UpdateValue(true);
                _interacting = false;
                _userHelpText.SetActive(false);
                _userHelpTextImage.SetActive(false);
            }
        }

        private void TutorialButtonPressed() {
            if(blockingTutorial)_movementEnabledEvent.UpdateValue(true);
            _userHelpText.SetActive(false);
            _userHelpTextImage.SetActive(false);
            this.gameObject.SetActive(false);
        }
        
        static string GetEnumDescription(Enum value) {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : value.ToString();
        }

        public void ShowHelpText(string text1 = "", string text2 = "") {
            if (_userHelpText == null) return;
            _userHelpTextImage.SetActive(true);
            
            _userHelpText.SetActive(true);
            _userHelpText.GetComponent<TextMeshProUGUI>().text = text1 + " [" + _interactionActionName + "] " + text2;
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
        
    }
    
}