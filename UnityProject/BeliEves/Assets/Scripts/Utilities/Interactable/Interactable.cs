using UnityEngine;

namespace Utilities.Interactable {
    public abstract class Interactable : MonoBehaviour {
        [SerializeField] protected InteractableGameObject interactable;
        public abstract void Interact();

        protected virtual void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<Player.Player>(out var player)) {
                this.interactable.enabled = true;
                OnTriggerEnterCallback();
            }
        }

        protected abstract void OnTriggerEnterCallback();

        private void OnTriggerExit(Collider other) {
            if (other.TryGetComponent<Player.Player>(out var player)) {
                this.interactable.enabled = false;
                OnTriggerExitCallback();
            }
        }

        protected abstract void OnTriggerExitCallback();
    }
}