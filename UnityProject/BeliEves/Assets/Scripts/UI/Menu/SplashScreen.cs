using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace UI.Menu {
    public class SplashScreen : Singleton<SplashScreen> {
        [FormerlySerializedAs("_menu")] [SerializeField] private GameObject menu;
        public void Update() {
            try {
                if (Input.anyKeyDown && !menu.activeInHierarchy) {
                    menu.SetActive(true);
                }
            }
            catch (Exception e) {
                // ignored
            }
        }
    }
}