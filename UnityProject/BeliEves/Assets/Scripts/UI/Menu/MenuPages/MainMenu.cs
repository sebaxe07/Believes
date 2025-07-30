using System;
using System.Collections;
using Managers;
using UI.Menu;
using UI.Transition;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MenuManagement {
    public class MainMenu : Menu<MainMenu> {
        public bool fadeToPlay = true;                          // should it use the fading transition?
        [SerializeField] private float playDelay = 1.5f;        // # seconds before loading the gameplay screen
        [SerializeField] private TransitionFader transitionFaderPrefab;

        [SerializeField] private string formUrl = "https://forms.office.com/pages/responsepage.aspx?id=K3EXCvNtXUKAjjCd8ope6yQYVmEP1cRBla4KfTsILBhURjRRVUNRTERMSUI4TE1NWDkyUlhWOUNKWS4u&route=shorturl";

        [SerializeField] private GameObject newGameBt;
        [SerializeField] private GameObject continueGameBt;
        [SerializeField] private GameObject arenaBt;
        [Header("Debug")]
        [SerializeField] private bool debug = false;

        private GameManager gameManager;
        private bool _gameStarted = false;

        public void Start() {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            transitionFaderPrefab = Resources.Load<TransitionFader>("Prefabs/Menu/Transition/TransitionScreen");

            Time.timeScale = 1f;
        }

        public void OnEnable() {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            (bool existSave, bool gameCompleted) = gameManager.SavesExist();
            if (debug) return;
            if (!existSave && !_gameStarted) {
                if (continueGameBt != null) continueGameBt.SetActive(false);
                if (arenaBt != null) arenaBt.SetActive(false);
            }
            else if (!gameCompleted) {
                if (continueGameBt != null) continueGameBt.SetActive(true);
                if (arenaBt != null) arenaBt.SetActive(false);
            }
            else {
                if (continueGameBt != null) continueGameBt.SetActive(false);
                if (arenaBt != null) arenaBt.SetActive(true);
            }
        }

        public void OnSettingsPressed() {
            //print("SETTINGS");
            SettingsMenu.Open();
        }

        public void OnCreditPressed() {
            gameManager.CreditsSceneOperation.allowSceneActivation = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnPlayPressed() {
            _gameStarted = true;
            StartCoroutine(PlayRoutine(() => gameManager.LoadLastSave()));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void onFeedbackPressed() {
            Application.OpenURL(formUrl);
        }

        public void OnNewGamePressed() {
            _gameStarted = true;
            StartCoroutine(PlayRoutine(() => gameManager.NewGame()));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnArenaPressed() {
            StartCoroutine(PlayRoutine(() => gameManager.NewArena()));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private IEnumerator PlayRoutine(Action newSceneLoader) {
            TransitionFader.PlayTransition(transitionFaderPrefab, "Loading...");
            yield return new WaitForSeconds(playDelay);

            newSceneLoader();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == "MainMenu") return;
            base.OnBackPressed();
        }


        public override void OnBackPressed() {
            Application.Quit();
        }
    }
}
