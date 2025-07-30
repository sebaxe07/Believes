using System.Collections;
using Events;
using Events.EventsLayout;
using Managers;
using UnityEngine;

namespace UI {
    public class PlaceHolderFadeOut : MonoBehaviour {
        private BasicEventChannel _nextStorySceneChannel;
        private bool _used = false;
        private void Awake() {
            _nextStorySceneChannel = EventBroker.TryToAddEventChannel("nextStorySceneChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
        }

        private void Start() {
            var gameManager = FindObjectOfType<GameManager>();
            gameManager.CreditScenePreload();
        }
        private void Update() {
            if (Input.anyKeyDown && !_used) {
                _used = true;
                _nextStorySceneChannel.RaiseEvent();
            }
        }
        
    }
}