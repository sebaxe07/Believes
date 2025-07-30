using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

public class MusicChanger : MonoBehaviour {

    [SerializeField] private SoundID _id;

    [Header("All Music")]
    [SerializeField] public bool AllMusic = false;

    private MusicController musicController;

    private void Start() {
        musicController = MusicController.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Player.Player>(out var player)) {
            if (AllMusic) {
                musicController.StartRandomSong();
            }
            else {
                musicController.PlaySong(_id);
            }
        }
    }

}
