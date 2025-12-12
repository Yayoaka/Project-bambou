using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyGhostRigSystem : MonoBehaviour
    {
        [SerializeField] private SerializableDictionary<string, Transform[]> ghostRigAnimators;
        
        #region Singleton

        public static EnemyGhostRigSystem Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        
        #endregion

        public Dictionary<string, Transform[]> GetGhostRigs => ghostRigAnimators;
    }
    
    [Serializable] //TODO Dégager cette merde d'ici pleaaaaaasssssseeeeee
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<Entry> entries = new List<Entry>();

        [Serializable]
        private struct Entry
        {
            public TKey key;
            public TValue value;
        }

        // Avant que Unity sérialise l'objet (Editor -> disque, playmode, etc.)
        public void OnBeforeSerialize()
        {
            entries.Clear();
            foreach (var kvp in this)
            {
                entries.Add(new Entry { key = kvp.Key, value = kvp.Value });
            }
        }

        // Après que Unity a désérialisé l'objet
        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                var key = entries[i].key;

                // Attention: si clé dupliquée, on garde la dernière (ou ignore, à choix)
                if (ContainsKey(key))
                {
                    // Option 1: écraser
                    this[key] = entries[i].value;

                    // Option 2: ignorer (décommentez si vous préférez)
                    // continue;
                }
                else
                {
                    Add(key, entries[i].value);
                }
            }
        }
    }
}
