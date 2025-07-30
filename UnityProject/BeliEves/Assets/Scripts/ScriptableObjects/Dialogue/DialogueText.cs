using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptableObjects.Dialogue {
    [CreateAssetMenu (menuName="ScriptableObjects/Dialogue/DialogueText")]
    public class DialogueText  : ScriptableObject{
        public string speakerName;
        public Sprite speakerImageReference;
        
        [TextArea(5,10)]
        public List<string> paragraph;
        
        private Stack<string> paragraphStack = new Stack<string>();

        private void OnValidate() {
            FillStack();
        }

        public void Reset() {
            FillStack();
        }
        
        public virtual (string, bool) ConsumeParagraph() {
            return (paragraphStack.Pop(), paragraphStack.Count == 0);
        }

        private void FillStack() {
            paragraphStack.Clear();

            for (int i = paragraph.Count - 1; i >= 0; i--) {
                paragraphStack.Push(paragraph[i]);
            }
        }
    }
}