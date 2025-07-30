using System;
using System.Collections;
using Ami.BroAudio;
using Events;
using Events.EventsLayout;
using UnityEngine;

namespace Controller.Stats.Bonus {
    public class BatteryComponent : MonoBehaviour {
        [SerializeField] private float batteryLife = 12;
        [SerializeField] private SoundID _batteryPickupSound = default;

        //events
        private BasicEventChannel _batteryEventChannel;


        private void Start() {
            this.OnEnable();
        }

        private void OnEnable() {
            StartCoroutine(LifeCoroutine(batteryLife));

            //setup events
            _batteryEventChannel = EventBroker.TryToAddEventChannel("BatteryEventChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
        }

        private void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent<Player.Player>(out var player)) return;
            _batteryEventChannel.RaiseEvent();
            BroAudio.Play(_batteryPickupSound);
            gameObject.SetActive(false);
        }

        private IEnumerator LifeCoroutine(float seconds) {
            if (batteryLife != 0) { //if you set battery life to zero the battery do not despawn in time
                yield return new WaitForSeconds(seconds);
                gameObject.SetActive(false);
            }
        }
    }
}