using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using ScriptableObjects.Dialogue;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utilities.Events.EventsLayout;
using Utilities.Input;
using Utilities.Interactable;

namespace Managers.DialogueManager {
    public class DialogueManager : Interactable {
        [Header("leftDialogueBox")]
        [SerializeField] private GameObject leftDialogueBox;
        [SerializeField] private TextMeshProUGUI leftNameText;
        [SerializeField] private TextMeshProUGUI leftDialogueText;
        [Header("rightDialogueBox")]
        [SerializeField] private GameObject rightDialogueBox;
        [SerializeField] private TextMeshProUGUI rightNameText;
        [SerializeField] private TextMeshProUGUI rightDialogueText;
        [SerializeField] private GameObject rightPgImageGameObj;
        private UnityEngine.UI.Image _rightPgImage;
        [Header("type Settings")]
        [SerializeField] private float typeSpeed = 9f;
        [SerializeField] private float maxTypeTime = 0.1f;

        private EventWithAction _dialogueHelperEvent;
        private Action _helperCallback;

        private static readonly List<string> Actions = new List<string> {
            "Move", "Jump", "Run",
            "lightAttack", "HeavyAttack", "SpecialAttack",
            "EveRelease"
        };

        private bool _isTyping;
        private DialogueSequence _dialogue;

        private string _currentDialogue;

        private Coroutine _typeCoroutine;
        private Action _endDialogueAction;


        public void Start() {
            _dialogueHelperEvent = (EventWithAction)EventBroker.TryToAddEventChannel("dialogueHelperEvent", ScriptableObject.CreateInstance<EventWithAction>());
            _dialogueHelperEvent.Subscribe(new Action(() => { UpdateDialogueHelperCallback(_dialogueHelperEvent.EventAction); }));

            _rightPgImage = rightPgImageGameObj.GetComponent<UnityEngine.UI.Image>();
        }

        public void Update() {
            if (_helperCallback != null) _helperCallback.Invoke();
        }

        private void UpdateDialogueHelperCallback(Action action) {
            _helperCallback = action;
        }

        public void DisplayDialogue(DialogueSequence dialogue, Action endDialogueAction = null) {
            // Debug.Log("DisplayDialogue");
            if (dialogue.IsConversationEnded()) {
                // Debug.LogError("Returning from DisplayDialogue");
                EndEveConversation();
                EndNpcConversation();
                return;
            }
            _endDialogueAction = endDialogueAction;

            // Debug.LogWarning("Disabling actions");
            InputEnabler.DisableActions(Actions, FindObjectOfType<PlayerInput>());

            //start new Conversation
            _dialogue = dialogue;
            VoidDisplayDialogue(_dialogue.ConsumeParagraph());
        }
        public override void Interact() {
            if (_isTyping) {
                StopCoroutine(_typeCoroutine);
                rightDialogueText.text = _currentDialogue;
                rightDialogueText.maxVisibleCharacters = _currentDialogue.Length;
                _isTyping = false;
                return;
            }
            if (_dialogue.IsConversationEnded()) {
                interactable.HideHelpText();
                EndEveConversation();
                EndNpcConversation();
                interactable.HideHelpText();

                if (_endDialogueAction != null) _endDialogueAction();

                InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
                return;
            }

            //display new message.
            VoidDisplayDialogue(_dialogue.ConsumeParagraph());
        }

        private void StartEveConversation() {
            EndNpcConversation();

            interactable.enabled = true;
            leftDialogueBox.SetActive(true);
        }

        private void EndEveConversation() {
            interactable.enabled = false;
            leftDialogueBox.SetActive(false);
        }

        private void StartNpcConversation() {
            EndEveConversation();

            interactable.enabled = true;
            rightDialogueBox.SetActive(true);
        }

        private void EndNpcConversation() {
            interactable.enabled = false;
            rightDialogueBox.SetActive(false);
        }

        private void VoidDisplayDialogue(DialogueTextStruct? dialogue) {
            if (dialogue == null) {
                PrepareHelperDialogue();
                return;
            }

            var currentDialogue = dialogue.Value;

            _currentDialogue = currentDialogue.text;

            if (!leftDialogueBox.activeSelf && currentDialogue.speakerName == "Eve") {
                StartEveConversation();
            }

            if (currentDialogue.speakerName == "Eve") {
                leftNameText.text = currentDialogue.speakerName;
                if (_typeCoroutine != null) {
                    StopCoroutine(_typeCoroutine);
                }
                _typeCoroutine = StartCoroutine(TypeDialogueText(currentDialogue.text, leftDialogueText));
            }

            if (!rightDialogueBox.activeSelf && currentDialogue.speakerName != "Eve") {
                StartNpcConversation();
            }

            if (currentDialogue.speakerName != "Eve") {
                rightNameText.text = currentDialogue.speakerName;
                if (currentDialogue.speakerImageReference != null) {
                    rightPgImageGameObj.SetActive(true);
                    _rightPgImage.sprite = currentDialogue.speakerImageReference;
                }
                else {
                    rightPgImageGameObj.SetActive(false);
                }
                if (_typeCoroutine != null) {
                    StopCoroutine(_typeCoroutine);
                }
                _typeCoroutine = StartCoroutine(TypeDialogueText(currentDialogue.text, rightDialogueText));
            }
        }

        private void PrepareHelperDialogue() {
            EndEveConversation();
            EndNpcConversation();
            interactable.HideHelpText();
            _isTyping = true;

            if (_endDialogueAction != null) _endDialogueAction();
            InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
        }

        private void HelperDialogueExit() {
            //TODO
        }


        protected override void OnTriggerEnterCallback() { }

        protected override void OnTriggerExitCallback() { }

        private IEnumerator TypeDialogueText(string p, TextMeshProUGUI text) {
            interactable.ShowHelpText("Press ", " to skip");
            _isTyping = true;

            int maxVisibleChars = 0;

            text.text = p;
            text.maxVisibleCharacters = maxVisibleChars;

            foreach (char c in p.ToCharArray()) {

                maxVisibleChars++;
                text.maxVisibleCharacters = maxVisibleChars;

                yield return new WaitForSeconds(maxTypeTime / typeSpeed);
            }

            _isTyping = false;
            //interactable.HideHelpText();
        }
    }
}