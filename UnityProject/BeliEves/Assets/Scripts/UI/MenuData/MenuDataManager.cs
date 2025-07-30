using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.DataManager {   
    public class MenuDataManager : Singleton<MenuDataManager> {
        [SerializeField] private SerializableDictionary<string,string> serializableDictionary;
        [SerializeField] private string fileRelativePath = "";

        
        private JsonSaver _jsonSaver;
        
        private void Awake() {
            //DontDestroyOnLoad(gameObject);
            _jsonSaver = new JsonSaver();
        }

        public void Save(GenericData saveData, string dictionaryKey) {
            _jsonSaver.Save(saveData, serializableDictionary.GetValue(dictionaryKey), fileRelativePath);
        }

        public void Load(GenericData saveData, string dictionaryKey) {
            bool loaded = _jsonSaver.Load(saveData, serializableDictionary.GetValue(dictionaryKey), fileRelativePath);

            if (loaded) return;
            saveData = new DataAudio();
            Save(saveData, dictionaryKey);
        }
    }
}
