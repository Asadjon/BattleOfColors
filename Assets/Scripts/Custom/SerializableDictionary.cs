using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Custom
{

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private bool m_IsModifiedCount;
        [SerializeField] private List<SerializationKeyValuePair> m_List = new List<SerializationKeyValuePair>();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (m_IsModifiedCount) return;
            m_List.Clear();
            ForEach(keyValuePair => m_List.Add((SerializationKeyValuePair)keyValuePair));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            try
            {
                m_List.ForEach(keyValuePair => {
                    if (!ContainsKey(keyValuePair.Key))
                        Add(keyValuePair.Key, keyValuePair.Value);
                });
            }
            catch (ArgumentException) {}
        }

        public void ForEach(Action<KeyValuePair<TKey, TValue>> action)
        { foreach (var kvp in this) action?.Invoke(kvp); }


        [Serializable] private struct SerializationKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public static implicit operator KeyValuePair<TKey, TValue>(SerializationKeyValuePair dictionary) => new KeyValuePair<TKey, TValue>(dictionary.Key, dictionary.Value);

            public static explicit operator SerializationKeyValuePair(KeyValuePair<TKey, TValue> keyValuePair) => new SerializationKeyValuePair 
            { 
                Key = keyValuePair.Key, 
                Value = keyValuePair.Value 
            };
        }
    }
}