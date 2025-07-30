using System;
using System.Collections.Generic;

namespace Utilities {
    [Serializable]
    public class SerializableDictionary<TKey, TValue>{ 
        public List<SerializableKeyValuePair<TKey, TValue>> KeyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();

        public string GetValue(string key) {
            foreach (var pair in KeyValuePairs) {
                if(pair.Key.Equals(key)) return pair.Value.ToString();
            }
            return null;
        }
    }

    [Serializable]
    public struct SerializableKeyValuePair<TKey, TValue>{
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair(TKey key, TValue value) {
            Key = key;
            Value = value;
        }
    }
}