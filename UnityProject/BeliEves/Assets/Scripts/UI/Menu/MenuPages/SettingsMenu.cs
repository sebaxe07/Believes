using UI.Menu;
using UnityEngine;
using AudioSettings = UI.Menu.AudioSettings;

namespace MenuManagement {
    public class SettingsMenu: Menu<SettingsMenu> {
        
        [SerializeField] private GameObject audioSettings;
        [SerializeField] private GameObject inputSettings;
        protected override void Awake() {
            base.Awake();
            friendlyName = "Settings";
        }

        public void OnAudioSettingsPressed() {
            audioSettings.SetActive(true);
            inputSettings.SetActive(false);
        }

        public void OnInputSettingsPressed() {
            audioSettings.SetActive(false);
            inputSettings.SetActive(true);
            audioSettings.GetComponent<AudioSettings>().SaveData();
        }
        
        public override void OnBackPressed() {
            base.OnBackPressed();
            audioSettings.GetComponent<AudioSettings>().SaveData();
        }
    }
}