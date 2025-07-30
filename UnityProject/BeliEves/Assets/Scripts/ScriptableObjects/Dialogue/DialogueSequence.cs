using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace ScriptableObjects.Dialogue {

    public struct DialogueTextStruct {
        public string text;
        public string speakerName;
        public Sprite speakerImageReference;
    }
    
    [CreateAssetMenu (menuName="ScriptableObjects/Dialogue/DialogueSequence")]
    public class DialogueSequence : ScriptableObject{
        public DialogueText[] dialogueSequence;
        
        private int currentDialogueText;
        private bool conversationStarted;

        //reset on game start
        private void OnValidate() {
            currentDialogueText = 0; 
            conversationStarted = false;
        }

        public void ResetDialogueSequence() {
            currentDialogueText = 0; 
            conversationStarted = false;
            foreach (DialogueText dialogue in dialogueSequence) {
                dialogue.Reset();
            }
        }
        
        public DialogueTextStruct? ConsumeParagraph() {
            conversationStarted = true; //start conversation
            
            //consume text
            var currentParagraph = dialogueSequence[currentDialogueText].ConsumeParagraph();
            
            DialogueTextStruct dialogueText;
            dialogueText.text = currentParagraph.Item1;
            dialogueText.speakerName = dialogueSequence[currentDialogueText].speakerName;
            dialogueText.speakerImageReference = dialogueSequence[currentDialogueText].speakerImageReference;

            //skip to next paragraph
            if (currentParagraph.Item2) NextSentence();
            if (currentParagraph.Item1 == null) return null; 
            
            return dialogueText; 
        }

        public bool IsFistSequence() {
            return currentDialogueText == 0;
        }

        public bool IsConversationStarted() {
            return conversationStarted;
        }

        public bool IsConversationEnded() {
            return currentDialogueText >= dialogueSequence.Length;
        }

        private void NextSentence() {
            currentDialogueText++;
        }
    }
}