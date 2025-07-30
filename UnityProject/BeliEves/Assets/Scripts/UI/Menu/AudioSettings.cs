using Ami.BroAudio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Utilities;
using Utilities.DataManager;

namespace UI.Menu {
    public class AudioSettings : MonoBehaviour {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        private MenuDataManager _menuDataManager;
        private DataAudio _audioSettings;
        private readonly string _dictionaryKey = "audio";
        private bool _databaseLoaded = false;

        protected void Awake() {
            _menuDataManager = GameObject.FindObjectOfType<MenuDataManager>();
            _audioSettings = new DataAudio();
        }

        private void Start() {
            LoadData();
        }

        private void LoadData() {
            if (_menuDataManager == null || masterVolumeSlider == null ||
                sfxVolumeSlider == null || musicVolumeSlider == null)
                return;

            _menuDataManager.Load(_audioSettings, _dictionaryKey);

            masterVolumeSlider.value = _audioSettings.MasterVolume;
            sfxVolumeSlider.value = _audioSettings.SfxVolume;
            musicVolumeSlider.value = _audioSettings.MusicVolume;
            _databaseLoaded = true;
            //UpdateAudioManager();
        }


        public void SaveData() {
            //update values
            _audioSettings.MasterVolume = masterVolumeSlider.value;
            _audioSettings.SfxVolume = sfxVolumeSlider.value;
            _audioSettings.MusicVolume = musicVolumeSlider.value;
            //save in json file
            _menuDataManager.Save(_audioSettings, _dictionaryKey);
        }

        public void SetMasterVolumes() {
            if (!_databaseLoaded) return;


            Debug.Log("Master volume: " + masterVolumeSlider.value);
            BroAudio.SetVolume(masterVolumeSlider.value);
        }

        public void SetSfxVolume() {
            if (!_databaseLoaded) return;

            Debug.Log("SFX volume: " + sfxVolumeSlider.value);
            BroAudio.SetVolume(BroAudioType.SFX, sfxVolumeSlider.value);
        }

        public void SetMusicVolume() {
            if (!_databaseLoaded) return;

            Debug.Log("Music volume: " + musicVolumeSlider.value);
            BroAudio.SetVolume(BroAudioType.Music, musicVolumeSlider.value);
        }

        /*public void UpdateAudioManager() {
            //TODO  update values in audio manager
        }*/
    }

}
