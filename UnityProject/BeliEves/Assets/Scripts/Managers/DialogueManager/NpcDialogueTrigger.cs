using System;
using Events;
using ScriptableObjects.Dialogue;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Managers.DialogueManager {
    public class NpcDialogueTrigger : MonoBehaviour {
        [SerializeField] private DialogueSequence dialogueSequence;
        [SerializeField] private DialogueManager dialogueManager;

        [SerializeField] private int frameAlloweOfset = 1;

        private EventBodySwitch _bodySwitchChannel;
        private int _frame = 0;

        void Start() {
            dialogueSequence.ResetDialogueSequence();
            
            //setting up Event
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
            _bodySwitchChannel.Subscribe(BodySwitchTime);
        }
        
        private void OnTriggerEnter(Collider other) {
            if (!this.TryGetComponent<Player.Player>(out var player) || (Time.frameCount - _frame > frameAlloweOfset)) return;
            
            dialogueManager.DisplayDialogue(dialogueSequence);
        }
        
        private void BodySwitchTime() {
            _frame =  Time.frameCount;
        }
    }
}