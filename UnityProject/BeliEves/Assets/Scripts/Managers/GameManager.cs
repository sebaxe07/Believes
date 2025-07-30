using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Events;
using Events.EventsLayout;
using Managers.Saves;
using ScriptableObjects.Dialogue;
using UI.Transition;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities.DataManager;
using Utilities.Events.EventsLayout;
using Ami.BroAudio;
using Cinemachine;
using Controller.CameraManager;
using Managers.CameraManager;
using Unity.VisualScripting;
using UnityEngine.AI;
using Utilities.Pool;

namespace Managers {
    public class ArenaSaveData {
        public Vector3 ArenaRespawnPosition = Vector3.zero;
        public bool InitialSceneEnd = false;
        public CheckpointArenaVariant LastActiveCheckpoint;
        public BodyType CurrentBodyType = BodyType.Eve;
        public BodyType InventoryBodyType = BodyType.Eve;
        public float InventorySavedHealth = 0f;
        public float InventorySavedStamina = 0f;
        public float Health;
        public float Stamina;
        public string CameraName;
        public float CameraAngle;
    }
    public class GameManager : Utilities.Singleton<GameManager> {
        [Header("Scenes")]
        [SerializeField] private List<string> startingSceneName = new List<string>();
        [SerializeField] private string arenaSceneName;
        [SerializeField] private String creditsSceneName = "CreditScene";
        public AsyncOperation CreditsSceneOperation;
        
        [Header("Save file location")]
        [SerializeField] private string fileName = "";
        [SerializeField] private string fileRelativePath = "";

        [Header("Death scene")]
        [SerializeField] private DialogueSequence deathSequence;
        
        [Header("Scene fade time")]
        [SerializeField]private float playDelay = 1.5f; 
        
        private string _currentScene;
        private int _storySceneNumber = 0;
        private Checkpoint _lastCheckpoint;
        private GameObject _player;
        private EventBodySwitch _bodySwitchChannel;
        private InventoryEventChannel _bodySaveChannel;
        private bool _saveExist = false;
        private SwitchCameraEvent _switchCameraEventChannel;

        private GameSaveData _gameSaveData;
        private JsonSaver _jsonSaver;

        private EventGetStats _getStatsEvent;
        private EventGetStats _setStatsEvent;
        private EventWithBool _fistGameLoadEvent;
        private InventoryEventChannel _preSaveChannel;
        private EventWithVector3 _deathEvent;
        private SwitchCameraEvent _currentCameraEventChannel;
        private BasicEventChannel _nextStorySceneChannel;
        private EventWithFloat _currentBatteryCount;

        private DialogueManager.DialogueManager _dialogueManager;
        //Arena Specific
        private ArenaSaveData _arenaSaveData = new ArenaSaveData();
        
        //switch scene
         
        private TransitionFader transitionFaderPrefab;
        
        public void Awake() {
            DontDestroyOnLoad(gameObject);
            transitionFaderPrefab = Resources.Load<TransitionFader>("Prefabs/Menu/Transition/TransitionScreen");
            
            _gameSaveData = new GameSaveData();
            _jsonSaver = new JsonSaver();
            _saveExist = LoadLastSaveFiles();
            if (_saveExist) _storySceneNumber = _gameSaveData.StorySceneNumber;
            if(_storySceneNumber>=startingSceneName.Count)return;
            //prepareScene
            _currentScene = startingSceneName[_storySceneNumber];

            deathSequence.ResetDialogueSequence();
        }

        public void Start() {
            SetAllAudioVolumes();
            CreditScenePreload();
        }

        public void CreditScenePreload() {
            CreditsSceneOperation = SceneManager.LoadSceneAsync(creditsSceneName);
            CreditsSceneOperation!.allowSceneActivation = false;
        }

        public void NewArena() {
            EventBroker.ResetEventSystem();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneLoaded += OnAreaLoaded;
            SceneManager.LoadScene(arenaSceneName);
            _currentScene = arenaSceneName;
        }
        public void LoadLastSave() {
            if(_storySceneNumber>=startingSceneName.Count)return;
            EventBroker.ResetEventSystem();
            SceneManager.sceneLoaded += OnSceneLoaded;
            _currentScene = startingSceneName[_storySceneNumber];
            _saveExist = LoadLastSaveFiles();
            SceneManager.LoadScene(_currentScene);
        }

        public void NewGame() {
            RemoveAllSaveFiles();
            _currentScene = startingSceneName[0];
            _saveExist = false;
            _gameSaveData = new GameSaveData();
            _storySceneNumber = 0;
            
            EventBroker.ResetEventSystem();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(_currentScene);
        }

        private void NextStoryScene() {
            if (_storySceneNumber+1 >= startingSceneName.Count) {
                EndStory();
                return;
            }
            
            if(!startingSceneName[_storySceneNumber].Contains("intermezzo")){NextSceneSave();}
            PoolManager.ClearAllPools();
            _storySceneNumber += 1;
            var s = "Loading...";
            if (startingSceneName[_storySceneNumber].Contains("intermezzo")){ s = "what about now…";}
            

            

            StartCoroutine(PlayRoutine(LoadLastSave, s));
        }

        private void NextSceneSave() {
            //reset
            _gameSaveData.Position = Vector3.zero;
            _gameSaveData.DeathPosition = Vector3.zero;
            _gameSaveData.DeathBodyType = BodyType.Eve;
            _gameSaveData.CameraName = null;
            _gameSaveData.CameraAngle = 0f;
            
            //set
            _gameSaveData.BodyType = _bodySwitchChannel.bodyType;
            _gameSaveData.Health = _getStatsEvent.currentHealth;
            _gameSaveData.Stamina = _getStatsEvent.currentStamina;
            _gameSaveData.InventorySavedRobotType = _bodySaveChannel.bodyType;
            _gameSaveData.InventorySavedHealth = _bodySaveChannel.savedHealth;
            _gameSaveData.InventorySavedStamina = _bodySaveChannel.savedStamina;
            
            //scene switch specific
            _gameSaveData.IsFistSceneLoaded = true;
            _gameSaveData.StorySceneNumber = _storySceneNumber+1;
            
            DiskSaveData();
        }
        
        private void EndStory() {
            _gameSaveData.StorySceneNumber = _storySceneNumber;
            Debug.LogError(CreditsSceneOperation);
            CreditsSceneOperation = SceneManager.LoadSceneAsync(creditsSceneName);
            CreditsSceneOperation!.allowSceneActivation = true;
            PoolManager.ClearAllPools();
            DiskSaveData();
        }
        
        private IEnumerator PlayRoutine(Action newSceneLoader, String text) {
            TransitionFader.PlayTransition(transitionFaderPrefab, text);
            yield return new WaitForSeconds(playDelay);
            
            newSceneLoader();
        }

        private void EventLoad() {
            //setting up Event
            _bodySwitchChannel = (EventBodySwitch)EventBroker.TryToAddEventChannel("bodySwitch",
                ScriptableObject.CreateInstance<EventBodySwitch>());
            _fistGameLoadEvent = (EventWithBool)EventBroker.TryToAddEventChannel("fistGameLoadEvent",
                ScriptableObject.CreateInstance<EventWithBool>());
            _fistGameLoadEvent.eventBool = true;
            _getStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("getStatsEvent",
                ScriptableObject.CreateInstance<EventGetStats>());
            _setStatsEvent = (EventGetStats)EventBroker.TryToAddEventChannel("setStatsEvent",
                ScriptableObject.CreateInstance<EventGetStats>());
            _bodySaveChannel = (InventoryEventChannel)EventBroker.TryToAddEventChannel("bodySave",
                ScriptableObject.CreateInstance<InventoryEventChannel>());
            _preSaveChannel = (InventoryEventChannel)EventBroker.TryToAddEventChannel("PreSave",
                ScriptableObject.CreateInstance<InventoryEventChannel>());
            _deathEvent = (EventWithVector3)EventBroker.TryToAddEventChannel("deathEvent",
                ScriptableObject.CreateInstance<EventWithVector3>());
            _deathEvent.Subscribe(new Action((() => { ManageDeath(_deathEvent.vector); })));
            _currentBatteryCount = (EventWithFloat)EventBroker.TryToAddEventChannel("CurrentBatteryCount", ScriptableObject.CreateInstance<EventWithFloat>());
            
            _switchCameraEventChannel = (SwitchCameraEvent)EventBroker.TryToAddEventChannel("SwitchCameraEventChannel", ScriptableObject.CreateInstance<SwitchCameraEvent>());
            _currentCameraEventChannel = (SwitchCameraEvent)EventBroker.TryToAddEventChannel("CurrentCameraEventChannel", ScriptableObject.CreateInstance<SwitchCameraEvent>());

            _nextStorySceneChannel = EventBroker.TryToAddEventChannel("nextStorySceneChannel", ScriptableObject.CreateInstance<BasicEventChannel>());
            _nextStorySceneChannel.Subscribe(NextStoryScene);
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name is "MainMenu" or "CreditScene") {
                _arenaSaveData = new ArenaSaveData();
                return;
            }

            SetAllAudioVolumes();
            EventBroker.SetCallBackHandler();

            EventLoad();

            _dialogueManager = FindObjectOfType<DialogueManager.DialogueManager>();
            if (scene.name != startingSceneName[_storySceneNumber]) return;
            if (_saveExist) {
                _fistGameLoadEvent.UpdateValue(false);
                if (!_gameSaveData.IsFistSceneLoaded) {
                    LoadPlayerPosition();
                    LoadRobotInDeathPosition();
                    LoadBattery(_gameSaveData.BatteryCount);
                } else if (_gameSaveData.BodyType != BodyType.Eve) {
                    FirstSceneLoadBodySwitch();
                }
                _setStatsEvent.UpdateValue(this.gameObject, _gameSaveData.Health, _gameSaveData.Stamina, true);
                _preSaveChannel.UpdateValue(_gameSaveData.InventorySavedRobotType, _gameSaveData.InventorySavedStamina, _gameSaveData.InventorySavedHealth);

                //load current camera
                if (_gameSaveData.CameraName != null) {
                    LoadLastCamera(_gameSaveData.CameraName, _gameSaveData.CameraAngle);
                }
            }

        }

        #region Audio

        private float[] LoadData() {
            var menuDataManager = GameObject.FindObjectOfType<MenuDataManager>();
            var audioSettings = new DataAudio();

            menuDataManager.Load(audioSettings, "audio");

            var masterVolume = audioSettings.MasterVolume;
            var sfxVolume = audioSettings.SfxVolume;
            var musicVolume = audioSettings.MusicVolume;

            return new float[] { masterVolume, sfxVolume, musicVolume };
        }


        //TODO: change volumes when in the menu, for music.
        private void SetAllAudioVolumes() {
            // // Debug.LogWarning("Setting all audio volumes");
            var audioSettings = LoadData();
            if (audioSettings == null) return;

            SetMasterVolume(audioSettings[0]);
            SetSfxVolume(audioSettings[1] * audioSettings[0]);
            SetMusicVolume(audioSettings[2] * audioSettings[0]);
        }

        private void SetMasterVolume(float volume) {
            // // Debug.LogError("Setting Master volume");
            BroAudio.SetVolume(volume);
        }
        private void SetSfxVolume(float volume) {
            // // Debug.LogError("Setting SFX volume");
            BroAudio.SetVolume(BroAudioType.SFX, volume, 8);
        }

        private void SetMusicVolume(float volume) {
            // // Debug.LogError("Setting Music volume");
            BroAudio.SetVolume(BroAudioType.Music, volume, 1);
        }

        #endregion

        #region Arena


        private void OnAreaLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name != arenaSceneName) return;

            var player = FindObjectOfType<Player.Player>().gameObject;
            if (_arenaSaveData.ArenaRespawnPosition == Vector3.zero || !_arenaSaveData.InitialSceneEnd) {
                _arenaSaveData.ArenaRespawnPosition = player.transform.position;
                return;
            }

            //load current body type after death
            switch (_arenaSaveData.CurrentBodyType) {
                case BodyType.Eve:
                    player.transform.position = _arenaSaveData.ArenaRespawnPosition;
                    break;
                case BodyType.Agile:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/AgileRobot Player Variant"), _arenaSaveData.CurrentBodyType, _arenaSaveData.ArenaRespawnPosition);
                    break;
                case BodyType.Tank:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/TankRobot Player Variant"), _arenaSaveData.CurrentBodyType, _arenaSaveData.ArenaRespawnPosition);
                    break;
                case BodyType.Support:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/SupportRobot Player Variant"), _arenaSaveData.CurrentBodyType, _arenaSaveData.ArenaRespawnPosition);
                    break;
            }

            //load stats after death
            _setStatsEvent.UpdateValue(this.gameObject, _arenaSaveData.Health, _arenaSaveData.Stamina, true);

            //load Inventory after death
            if (_arenaSaveData.InventoryBodyType != BodyType.Eve) {
                _preSaveChannel.UpdateValue(_arenaSaveData.InventoryBodyType, _arenaSaveData.InventorySavedStamina, _arenaSaveData.InventorySavedHealth);
            }

            //LoadBattery(_arenaSaveData.BatteryCount);

            //load camera

            if (_arenaSaveData.CameraName != null) {
                LoadLastCamera(_arenaSaveData.CameraName, _arenaSaveData.CameraAngle);
            }

            var cameraFade = GameObject.Find("CameraFade");
            Destroy(cameraFade);
        }

        public void UpdateArenaRespawnData(CheckpointArenaVariant checkpoint, Action saveCallback) {
            //savePosition
            _arenaSaveData.InitialSceneEnd = true;
            _arenaSaveData.ArenaRespawnPosition = checkpoint.transform.position;
            //_arenaSaveData.BatteryCount = (int)_currentBatteryCount.value;
            
            //saveActiveCheckpoint
            if (_arenaSaveData.LastActiveCheckpoint != null) _arenaSaveData.LastActiveCheckpoint.Toggle(false);
            _arenaSaveData.LastActiveCheckpoint = checkpoint;

            //saveBodyTypes
            _arenaSaveData.CurrentBodyType = _bodySwitchChannel.bodyType;
            _arenaSaveData.InventoryBodyType = _bodySaveChannel.bodyType;
            _arenaSaveData.InventorySavedHealth = _bodySaveChannel.savedHealth;
            _arenaSaveData.InventorySavedStamina = _bodySaveChannel.savedStamina;

            //save stats
            _arenaSaveData.Health = _getStatsEvent.currentHealth;
            _arenaSaveData.Stamina = _getStatsEvent.currentStamina;

            //save camera
            if (_currentCameraEventChannel != null && _currentCameraEventChannel.DynamicCameraSettings != null) {
                _arenaSaveData.CameraName = _currentCameraEventChannel.DynamicCameraSettings.Camera.gameObject.name;
                _arenaSaveData.CameraAngle = _currentCameraEventChannel.DynamicCameraSettings.InputOffsetAngle;
            }
            saveCallback();
        }
        #endregion Arena

        private void ManageDeath(Vector3 deathPose) {
            if (deathPose != Vector3.zero) {
                StartCoroutine(DeathSaveRoutine(deathPose));
            }
            
            deathSequence.ResetDialogueSequence();
            _dialogueManager.DisplayDialogue(deathSequence,
                new Action(() => {
                    StartCoroutine(DeathRoutine());
                }));
        }

        private IEnumerator DeathSaveRoutine(Vector3 deathPose) {
            _gameSaveData.DeathPosition = deathPose;
            _gameSaveData.DeathBodyType = _bodySwitchChannel.bodyType;
            DiskSaveData();
            yield return null;
        }

        private void LoadBattery(int value) {
            if(value <= 0) return;
            _currentBatteryCount.UpdateValue(value);
        }
        
        private void LoadLastCamera(String cameraName, float inputOffsetAngle) {
            var camEnter = GetCameraEnterCollider(cameraName);
            
            CinemachineVirtualCamera[] allCameras = FindObjectsOfType<CinemachineVirtualCamera>(true);

            foreach (CinemachineVirtualCamera cam in allCameras) {
                if (cam.Name == cameraName) {
                    var cameraSettings = new DynamicCameraSettings();
                    cameraSettings.Camera = cam;
                    cameraSettings.InputOffsetAngle = inputOffsetAngle;
                    if (camEnter != null) {
                        //camEnter.SetActive();
                        _switchCameraEventChannel.UpdateValue(cameraSettings, camEnter.ExitCamera);
                    }
                    else _switchCameraEventChannel.UpdateValue(cameraSettings);
                    return;
                }
            }
        }

        private CamEnter GetCameraEnterCollider(String cameraName) {
            var player = FindObjectOfType<Player.Player>().gameObject;
            Vector3 playerPosition = player.transform.position;
            Collider[] colliders = Physics.OverlapSphere(playerPosition, 0.1f);
            foreach (Collider collider in colliders) {
                if (collider.TryGetComponent<CamEnter>(out CamEnter camEnter) && camEnter.GetCameraName()==cameraName) {
                    camEnter.SetActive();
                    return camEnter;
                }
            }

            return null; 
        }

        private IEnumerator DeathRoutine() {
            TransitionFader.PlayTransition(Resources.Load<TransitionFader>("Prefabs/Menu/Transition/TransitionScreen"), "Eve is dead!");
            PoolManager.ClearAllPools();
            yield return new WaitForSeconds(0.8f);

            EventBroker.ResetEventSystem();
            deathSequence.ResetDialogueSequence();
            SceneManager.LoadScene(_currentScene);
            if (_currentScene != arenaSceneName) _saveExist = LoadLastSaveFiles();
        }

        #region Manage saves
        public (bool,bool) SavesExist() {
            return (_saveExist, _storySceneNumber>=startingSceneName.Count);
        }
        private bool LoadLastSaveFiles() {
            string directoryPath = string.IsNullOrEmpty(fileRelativePath) ?
                Application.persistentDataPath :
                Path.Combine(Application.persistentDataPath, fileRelativePath);

            if (!Directory.Exists(directoryPath)) return false;
            string[] saveFiles = null;
            try {
                saveFiles = Directory.GetFiles(directoryPath, $"{fileName}_*");
            }
            catch (Exception e) {
                return false;
            }
            if (saveFiles.Length == 0) {
                return false;
            }

            // Get the most recent save file by timestamp
            string latestSaveFile = saveFiles.OrderByDescending(File.GetCreationTime).FirstOrDefault();

            // Load data
            if (latestSaveFile != null) {
                bool loaded = _jsonSaver.Load(_gameSaveData, Path.GetFileName(latestSaveFile), fileRelativePath);
                if (!loaded) {
                    return false;
                }
            }
            return true;
        }


        public void SaveGame(Checkpoint checkpoint, Action saveCallback) {
            //manage in-game actions
            if (_lastCheckpoint != null) _lastCheckpoint.Toggle(false);
            _lastCheckpoint = checkpoint;
            _lastCheckpoint.Toggle(true);

            //collect saving data
            _gameSaveData.Position = _lastCheckpoint.transform.position;
            _gameSaveData.BodyType = _bodySwitchChannel.bodyType;
            _gameSaveData.Health = _getStatsEvent.currentHealth;
            _gameSaveData.Stamina = _getStatsEvent.currentStamina;
            _gameSaveData.InventorySavedRobotType = _bodySaveChannel.bodyType;
            _gameSaveData.InventorySavedHealth = _bodySaveChannel.savedHealth;
            _gameSaveData.InventorySavedStamina = _bodySaveChannel.savedStamina;
            _gameSaveData.DeathPosition = Vector3.zero;
            _gameSaveData.DeathBodyType = BodyType.Eve;
            _gameSaveData.BatteryCount = (int)_currentBatteryCount.value;
            _gameSaveData.StorySceneNumber = _storySceneNumber;
            _gameSaveData.IsFistSceneLoaded = false;
            
            //save camera
            if (_currentCameraEventChannel != null && _currentCameraEventChannel.DynamicCameraSettings != null) {
                _gameSaveData.CameraName = _currentCameraEventChannel.DynamicCameraSettings.Camera.gameObject.name;
                _gameSaveData.CameraAngle = _currentCameraEventChannel.DynamicCameraSettings.InputOffsetAngle;
            }

            //disk save
            DiskSaveData();

            saveCallback();
        }

        private void DiskSaveData() {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string timestampedFileName = $"{fileName}_{timestamp}";
            _jsonSaver.Save(_gameSaveData, timestampedFileName, fileRelativePath);
        }
        
        private void RemoveAllSaveFiles() {
            // Determine the directory path
            string directoryPath = string.IsNullOrEmpty(fileRelativePath) ?
                Application.persistentDataPath :
                Path.Combine(Application.persistentDataPath, fileRelativePath);

            // Get list of save files

            string[] saveFiles;
            try {
                saveFiles = Directory.GetFiles(directoryPath, $"{fileName}_*");
            }
            catch (Exception e) {
                return;
            }

            if (saveFiles.Length == 0) return;

            // Delete each file
            foreach (string file in saveFiles) File.Delete(file);
        }

        private void LoadRobotInDeathPosition() {
            if(_gameSaveData.DeathPosition == Vector3.zero) return;
            Vector3 position = GetPoseOnNavMesh(_gameSaveData.DeathPosition);
            if(position==Vector3.zero)return;

            GameObject deathBody;
            switch (_gameSaveData.DeathBodyType) {
                case BodyType.Agile:
                    deathBody = Resources.Load<GameObject>("Prefabs/Robots/AgileRobot");
                    break;
                case BodyType.Tank:
                    deathBody = Resources.Load<GameObject>("Prefabs/Robots/TankRobot");
                    break;
                case BodyType.Support:
                    deathBody = Resources.Load<GameObject>("Prefabs/Robots/SupportRobot");
                    break;
                default:
                    return;
            }
            
            var spawnPointPrefab = Resources.Load<GameObject>("Prefabs/Spawner/SpawnerPoint");
            var spawnPointComponent = spawnPointPrefab.GetComponent<SpawnerPoint>();
            spawnPointComponent.SetPrefab(deathBody);
            spawnPointComponent.EnableWaypointsGeneration(new Vector2(3,3), 3, 5);      
            var spawnPoint = Instantiate(spawnPointPrefab, position, Quaternion.identity).GetComponent<SpawnerPoint>();
            spawnPoint.Reinitialize();
            spawnPoint.TriggerSpawn();
        }
        
        private Vector3 GetPoseOnNavMesh(Vector3 position, float maxDistance = 5.0f) {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas)) {
                return hit.position;
            }
            else {
                return position;
            }
        }

        private void FirstSceneLoadBodySwitch() {
            var player = FindObjectOfType<Player.Player>();
            if (player == null) return;
            var pose = player.gameObject.transform.position;
            switch (_gameSaveData.BodyType) {
                case BodyType.Agile:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/AgileRobot Player Variant"), _gameSaveData.BodyType, pose);
                    break;
                case BodyType.Tank:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/TankRobot Player Variant"), _gameSaveData.BodyType, pose);
                    break;
                case BodyType.Support:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/SupportRobot Player Variant"), _gameSaveData.BodyType, pose);
                    break;
            }
        }
        private void LoadPlayerPosition() {
            GameObject robotPrefab = null;
            switch (_gameSaveData.BodyType) {
                case BodyType.Eve:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Eve"), _gameSaveData.BodyType, _gameSaveData.Position);
                    break;
                case BodyType.Agile:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/AgileRobot Player Variant"), _gameSaveData.BodyType, _gameSaveData.Position);
                    break;
                case BodyType.Tank:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/TankRobot Player Variant"), _gameSaveData.BodyType, _gameSaveData.Position);
                    break;
                case BodyType.Support:
                    LoadPlayerRobotPosition(Resources.Load<GameObject>("Prefabs/Robots/SupportRobot Player Variant"), _gameSaveData.BodyType, _gameSaveData.Position);
                    break;
            }
            LoadActivateCheckpoint(_gameSaveData.Position);
        }

        private void LoadPlayerRobotPosition(GameObject prefab, BodyType bodyType, Vector3 position) {
            var player = FindObjectOfType<Player.Player>().gameObject;
            Destroy(player);
            var robot = Instantiate(prefab, position, Quaternion.identity);
            robot.tag = "Player";
            _bodySwitchChannel.UpdateValue(bodyType);
        }

        private void LoadActivateCheckpoint(Vector3 targetPosition) {
            var colliders = Physics.OverlapSphere(targetPosition, 1);
            foreach (Collider collider in colliders) {
                if (collider.TryGetComponent<Checkpoint>(out var checkpoint)) {
                    checkpoint.Toggle(true);
                }
            }
        }



        #endregion
    }
}