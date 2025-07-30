using System;
using Events;
using Events.EventsLayout;
using UnityEngine;

namespace Managers {
    public class StorySceneSwitchTrigger: MonoBehaviour {
        private BasicEventChannel _nextStorySceneChannel;
        private void Awake() {
            _nextStorySceneChannel = EventBroker.TryToAddEventChannel("nextStorySceneChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
        }


        private void OnTriggerEnter(Collider other) {
            if(!other.TryGetComponent<Player.Player>(out var player)) return;
            Debug.LogError("next");
            _nextStorySceneChannel.RaiseEvent();
        }
    }
}