using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour {
    public static MusicController Instance { get; private set; }

    [SerializeField] public IAudioPlayer _player;
    [SerializeField] public bool ForceStopped = false;
    [SerializeField] private bool LastRandom = false;
    [SerializeField] public float _time = 5f;
    [SerializeField] public SoundID _Song1ID;
    [SerializeField] public SoundID _Song2ID;
    [SerializeField] public SoundID _Song3ID;
    [SerializeField] public SoundID _Song4ID;
    [SerializeField] public SoundID _Song5ID;
    [SerializeField] public SoundID _Song6ID;
    [SerializeField] public SoundID _Song7ID;
    [SerializeField] public SoundID _Song8ID;
    [SerializeField] public SoundID _Song9ID;
    [SerializeField] public SoundID _Song10ID;
    private readonly object _lock = new object();

    private void Awake() {
        // // Debug.Log("Music Controller Awake");
        if (Instance == null) {
            Instance = this;
            // Debug.LogWarning("Music Controller is the first instance in the scene ID " + gameObject.GetInstanceID());
            DontDestroyOnLoad(gameObject);


        }
        else {
            // Destroy script
            // Debug.LogWarning("Destroying Music Controller because it already exists in the scene ID " + gameObject.GetInstanceID());
            Destroy(this);
        }
    }

    private void Start() {
        // // Debug.Log("Music Controller Start");
        // Play random song when the game starts
        MainMenuLoop();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator DelayMenuMusic() {
        // // Debug.Log("Delaying menu music");
        yield return new WaitForSeconds(5f);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainMenu") {
            LastRandom = false;
            ForceStopped = false;
            // // Debug.Log("Main Menu loaded");
            // // Debug.LogWarning("Player is playing: " + _player.IsPlaying + " player id: " + _player.ID + " instance id: " + _player.GetHashCode());

            // // Debug.Log("Stoping current song because main menu is loaded");
            BroAudio.Stop(BroAudioType.Music);

            // // Debug.Log("Playing random song because main menu is loaded");
            // Play random song when main menu is loaded
            MainMenuLoop();
        }
        else {
            // // Debug.Log("Scene loaded: " + scene.name);
            if (scene.name != "ArenaTestScene") {
                // Debug.LogWarning("Scene is not ArenaTestScene setting last random to false " + LastRandom + " id " + gameObject.GetInstanceID());
                LastRandom = false;
            }
            else {
                // Debug.LogWarning("Scene is ArenaTestScene, not changing " + LastRandom + " id " + gameObject.GetInstanceID());

            }

            // // Debug.LogWarning("Player is playing: " + _player.IsPlaying + " player id: " + _player.ID + " instance id: " + _player.GetHashCode());
            // // Debug.Log("Stoping current song because scene is loaded");
            BroAudio.Stop(BroAudioType.Music);

        }
    }
    public void PlaySong(SoundID id) {
        if (ForceStopped) {
            ForceStopped = false;
            return;
        }
        // Check if the same song is playing
        if (_player != null && _player.ID == id && !LastRandom && _player.IsPlaying) {
            // // Debug.Log("Same song is playing");
            return;
        }
        // // Debug.Log("No song is playing or different song is playing or last song was random or looping");
        if (_player != null && _player.IsPlaying) {
            // Debug.Log("Forced Stop true One song");
            ForceStopped = true;
            BroAudio.Stop(_player.ID, _time);
        }
        LastRandom = false;
        _player = BroAudio.Play(id).OnEnd(new Action<SoundID>(_id => PlaySong(id)));
    }

    public void StartRandomSong() {
        // Debug.LogError("Player trying to start random song + " + LastRandom + " id " + gameObject.GetInstanceID());
        if (LastRandom) {
            // Debug.Log("Last song was random, returning " + LastRandom + " id " + gameObject.GetInstanceID());
            return;
        }
        // Debug.Log("Starting random song " + LastRandom + " id " + gameObject.GetInstanceID());
        PlayRandomSong();
    }

    public void PlayRandomSong() {
        if (SceneManager.GetActiveScene().name == "MainMenu") {
            // Debug.Log("Main Menu is active, returning");
            return;
        }
        // // Debug.LogError("Trying to play random song " + ForceStopped);
        if (ForceStopped) {
            // // Debug.Log("Force stopped, returning");
            ForceStopped = false;
            return;
        }
        LastRandom = true;

        if (_player != null && _player.IsPlaying) {
            ForceStopped = true;
            BroAudio.Stop(_player.ID, _time);
        }
        // Randomize the song 
        System.Random random = new System.Random();
        int song = random.Next(1, 11);
        // Debug.LogWarning("NEW Random song: " + song);
        switch (song) {
            case 1:
                _player = BroAudio.Play(_Song1ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 2:
                _player = BroAudio.Play(_Song2ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 3:
                _player = BroAudio.Play(_Song3ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 4:
                _player = BroAudio.Play(_Song4ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 5:
                _player = BroAudio.Play(_Song5ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 6:
                _player = BroAudio.Play(_Song6ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 7:
                _player = BroAudio.Play(_Song7ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 8:
                _player = BroAudio.Play(_Song8ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 9:
                _player = BroAudio.Play(_Song9ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
            case 10:
                _player = BroAudio.Play(_Song10ID).OnEnd(new Action<SoundID>(id => PlayRandomSong()));
                break;
        }

    }
    public void MainMenuLoop() {
        // // Debug.LogWarning("Main Menu Loop");
        if (SceneManager.GetActiveScene().name != "MainMenu") {
            // Debug.Log("Scene is not main menu, returning");
            return;
        }
        System.Random random = new System.Random();
        int song = random.Next(1, 11);
        // // Debug.LogWarning("Random Menu " + song);
        switch (song) {
            case 1:
                _player = BroAudio.Play(_Song1ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 2:
                _player = BroAudio.Play(_Song2ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 3:
                _player = BroAudio.Play(_Song3ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 4:
                _player = BroAudio.Play(_Song4ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 5:
                _player = BroAudio.Play(_Song5ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 6:
                _player = BroAudio.Play(_Song6ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 7:
                _player = BroAudio.Play(_Song7ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 8:
                _player = BroAudio.Play(_Song8ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 9:
                _player = BroAudio.Play(_Song9ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
            case 10:
                _player = BroAudio.Play(_Song10ID).OnEnd(new Action<SoundID>(id => MainMenuLoop()));
                break;
        }

    }

}
