using System;
using System.Collections.Generic;
using UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utilities.Input;
using Utilities.Pool;

namespace MenuManagement {
    public class PauseMenu : Menu<PauseMenu> {
        private static readonly List<string> Actions = new List<string> {
            "Move", "Jump", "Run", "Interaction",
            "lightAttack", "HeavyAttack", "SpecialAttack", "Perry",
            "EveRelease"
        };

        public void OnEnable() {
            Time.timeScale = 0f;
            InputEnabler.DisableActions(Actions, FindObjectOfType<PlayerInput>());

            StopAllCoroutines();
        }

        public override void SecurityAction() {
            Time.timeScale = 1f;
            InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
        }

        public void OnResumePressed() {            
            Time.timeScale = 1f;
            InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
            base.OnBackPressed();
        }
        public void OnMainMenuPressed() {
            Time.timeScale = 1f;
            InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
            PoolManager.ClearAllPools();
            Time.timeScale = 1f;
            base.OnBackPressed();
            SceneManager.LoadScene("MainMenu");
            MainMenu.Open();
        }
        public void OnSettingsPressed() {
            SettingsMenu.Open();
        }

        public void OnQuitPressed() {
            Time.timeScale = 1f;
            InputEnabler.EnableActions(Actions, FindObjectOfType<PlayerInput>());
            Application.Quit();
        }
    }
}
