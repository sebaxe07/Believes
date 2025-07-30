using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using Utilities.Events.EventsLayout;
using Utilities.Input;
using UnityEngine.InputSystem;
using System;
using Managers.DialogueManager;
using ScriptableObjects.Dialogue;

public class StopMovementTrigger : MonoBehaviour {
    [SerializeField] private DialogueSequence dialogueSequence;
        
    [SerializeField, Tooltip("Reference to the Dialogue UI Manager")] private DialogueManager dialogueManager;

    private EventWithBool _movementEnableEvent;
    private static readonly List<string> Actions = new List<string> {
            "Move", "Jump", "Run",
            "lightAttack", "HeavyAttack", "SpecialAttack"
        };
    private EventWithBool _fistGameLoadEvent;

    private EventBodySwitch _bodySwitchChannel;
    private void Start() {
        dialogueSequence.ResetDialogueSequence();
        _movementEnableEvent = (EventWithBool)EventBroker.TryToAddEventChannel("movementEnableEvent", ScriptableObject.CreateInstance<EventWithBool>());
        _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch", ScriptableObject.CreateInstance<EventBodySwitch>());
        _bodySwitchChannel.Subscribe(ReleaseLock);
        
        _fistGameLoadEvent = (EventWithBool)EventBroker.TryToAddEventChannel("fistGameLoadEvent", ScriptableObject.CreateInstance<EventWithBool>());

    }

    private void OnTriggerEnter(Collider other) {
        if(!_fistGameLoadEvent.eventBool || !other.TryGetComponent<Player.Player>(out var player))return;
        InputEnabler.DisableActions(Actions, FindObjectOfType<PlayerInput>(), this.GetHashCode().ToString());
        dialogueManager.DisplayDialogue(dialogueSequence);
        //_movementEnableEvent.UpdateValue(false);
    }
    
    private void ReleaseLock() {
        //_movementEnableEvent.UpdateValue(true);
        InputEnabler.ReleaseExclusivity(this.GetHashCode().ToString());
        Destroy(this.gameObject);
    }
}




