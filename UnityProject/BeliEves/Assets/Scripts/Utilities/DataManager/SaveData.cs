using Managers.Saves;
using UnityEngine;
using Utilities.Events.EventsLayout;

namespace Utilities.DataManager {
    [System.Serializable]
    public abstract class GenericData {
        public string hashValue;
    }
    public class DataAudio : GenericData {
        public float MasterVolume =1;
        public float SfxVolume =1;
        public float MusicVolume =1;
    }

    public class GameSaveData : GenericData {
        public Vector3 Position = Vector3.zero;
        
        public float Health = 0f;
        public float Stamina = 0f;
        
        public BodyType BodyType = BodyType.Eve;
        public BodyType InventorySavedRobotType = BodyType.Eve;
        
        public float InventorySavedHealth = 0f;
        public float InventorySavedStamina = 0f;
        
        public string CameraName;
        public float CameraAngle;
        
        public Vector3 DeathPosition = Vector3.zero;
        public BodyType DeathBodyType = BodyType.Eve;
        
        public bool IsFistSceneLoaded = false;
        public int StorySceneNumber = 0;

        public int BatteryCount = 0;
    }
}
