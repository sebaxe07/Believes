using System;
using ScriptableObjects.Dialogue;
using UnityEngine;

namespace Managers.DialogueManager {
    public class DialogueTrigger : MonoBehaviour {
        [SerializeField] private DialogueSequence dialogueSequence;
        
        [SerializeField, Tooltip("Reference to the Dialogue UI Manager")] private DialogueManager dialogueManager;

        private void Start() {
            dialogueSequence.ResetDialogueSequence();
        }

        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player)) return;
            
            dialogueManager.DisplayDialogue(dialogueSequence);
        }
    }
}