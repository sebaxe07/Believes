using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Utilities.DataManager {
    public class JsonSaver {
        private static readonly object _lockObject = new object();
        public static string GetSaveFilename(string filename, [CanBeNull] string  relativePath) {
            if(string.IsNullOrEmpty(relativePath))return Application.persistentDataPath + "/" + filename;
            return Application.persistentDataPath + "/" + relativePath + "/" + filename;
        }

        public void Save(GenericData data, string filename, [CanBeNull] string  relativePath) {
            lock (_lockObject) {
                data.hashValue = string.Empty;

                // generate the JSON data
                string json = JsonUtility.ToJson(data);

                // compute the hash and save it into the data structure
                data.hashValue = GetSHA256(json);

                // generate the JSON containing both the data and the hash value
                json = JsonUtility.ToJson(data);

                string saveFilename = GetSaveFilename(filename, relativePath);

                string directoryPath = Path.GetDirectoryName(saveFilename);
                if (!Directory.Exists(directoryPath)) {
                    Directory.CreateDirectory(directoryPath);
                }

                using var filestream = new FileStream(saveFilename, FileMode.Create);
                using var writer = new StreamWriter(filestream);
                writer.Write(json);
            }
        }

        public bool DataExists(string filename, [CanBeNull] string  relativePath) {
            return File.Exists(GetSaveFilename(filename,relativePath));    
        }

        public void CreateSaveData(GenericData data,string filename, [CanBeNull] string  relativePath) {
            Save(data, filename, relativePath);
        }
        
        public bool Load(GenericData data, string filename, [CanBeNull] string  relativePath) {
            lock (_lockObject) {
                string loadFilename = GetSaveFilename(filename, relativePath);
                string json;

                if (File.Exists(loadFilename)) {
                    FileStream filestream = new FileStream(loadFilename, FileMode.Open);

                    using (StreamReader reader = new StreamReader(filestream)) {
                        json = reader.ReadToEnd();

                        if (CheckData(json, data)) {
                            JsonUtility.FromJsonOverwrite(json, data);
                        }
                        else {
                            data = (GenericData)Activator.CreateInstance(data.GetType());
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        public void ClearSavedData(string filename, [CanBeNull] string  relativePath) {
            File.Delete(GetSaveFilename(filename,relativePath));
        }

        private string GetSHA256(string text) {
            byte[] textToBytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed mySHA256 = new SHA256Managed();
            byte[] hashValue = mySHA256.ComputeHash(textToBytes);

            return GetTextStringFromHash(hashValue);
        }

        private bool CheckData(string json, GenericData data) {
            GenericData tempSaveData = (GenericData)Activator.CreateInstance(data.GetType());
            
            JsonUtility.FromJsonOverwrite(json, tempSaveData);

            string oldHash = tempSaveData.hashValue;
            tempSaveData.hashValue = string.Empty;

            string tempJson = JsonUtility.ToJson(tempSaveData);
            string newHash = GetSHA256(tempJson);

            if (oldHash == newHash) {
                return true;
            }
            return false;
        }
        
        private string GetTextStringFromHash(byte[] hash) {
            string hexString = String.Empty;

            for (int i = 0; i < hash.Length; i++)
                hexString += hash[i].ToString("x2");

            return hexString;
        }
    }
}
