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

        public SerializableDictionary<string, Transform[]> GetGhostRigs => ghostRigAnimators;
    }
    
    [Serializable] //TODO Dégager cette merde d'ici pleaaaaaasssssseeeeee
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [Serializable]
        private struct Entry
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        // Dictionnaire runtime (non sérialisé)
        private readonly Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

        public Dictionary<TKey, TValue> Dictionary => dict;

        public void OnBeforeSerialize()
        {
            // IMPORTANT :
            // On NE DOIT PAS effacer/reconstruire "entries" à partir de dict ici,
            // sinon Unity perd ce que vous venez d'ajouter avec le bouton '+'.
            // Ici, on laisse Unity sérialiser la liste telle qu’elle est dans l’inspecteur.
        }

        public void OnAfterDeserialize()
        {
            dict.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                var k = entries[i].key;

                // Si doublon, on garde la dernière valeur (modifiable selon besoin)
                dict[k] = entries[i].value;
            }
        }

        // Helpers pratiques
        public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

        public TValue this[TKey key]
        {
            get => dict[key];
            set
            {
                dict[key] = value;
                // Si vous modifiez à runtime, cela ne met pas à jour l’inspecteur automatiquement.
            }
        }
    }
}
