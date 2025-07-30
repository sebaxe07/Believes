using System;
using System.ComponentModel;
using System.Reflection;
using Events;
using Managers.DialogueManager;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Events.EventsLayout;


namespace ScriptableObjects.Dialogue {
    [CreateAssetMenu (menuName="ScriptableObjects/Dialogue/DialogueHelper")]
    public class DialogueHelper: DialogueText {
        public InputType inputName;
        public string actionComment;
        public ActionType actionType;
        
        private GameObject _userHelpText;
        private GameObject _userHelpTextImage;
        private string _interactionActionName;
        private InputAction _interactionAction;
        private PlayerInput _playerInput;
        private bool _interacting;
        
        private EventWithAction _dialogueHelperEvent;
        
        public override (string, bool) ConsumeParagraph() {
            DoHelper();
            return (null, false);
        }

        private void PrepareHelper() {
            var bottomGameUI = GameObject.Find("BottomGameUI");
            if(bottomGameUI == null) return;
            _userHelpText = GetChildByName(bottomGameUI,"UserHelpText");
            _userHelpTextImage = GetChildByName(bottomGameUI,"BackgroundHelperTextImage");
            
            _playerInput = FindObjectOfType<PlayerInput>();
            _interactionAction = _playerInput.actions[inputName.ToString()];
            var control = InputSystem.FindControl(_interactionAction.bindings[0].effectivePath);
            _interactionActionName = control.name.ToUpper();
            
            _dialogueHelperEvent = (EventWithAction)EventBroker.TryToAddEventChannel("dialogueHelperEvent", ScriptableObject.CreateInstance<EventWithAction>());
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
        
        static string GetEnumDescription(Enum value) {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : value.ToString();
        }

        
        public void DoHelper() {
            _interacting = true;
            PrepareHelper();
            _dialogueHelperEvent.UpdateValue(HelperUpdate); 
            if (_userHelpText == null) {
                throw new NullReferenceException("UserHelpText is null");
                return;
            }
            
            _userHelpTextImage.SetActive(true);
            
            _userHelpText.SetActive(true);
            _userHelpText.GetComponent<TextMeshProUGUI>().text = GetEnumDescription(actionType)  + "[" + _interactionActionName + "]" + actionComment;
        }

        private void HelperUpdate() {
            if (_interacting && _interactionAction.WasPressedThisFrame()) HideHelpText();
        }
        
        private void HideHelpText() {
            _interacting = false;
            _userHelpTextImage.SetActive(false);
            _userHelpText.SetActive(false);
        }
    }
}