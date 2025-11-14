using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Global ScriptableObject registry initialized before the first scene loads.
    /// </summary>
    public static class GameDatabase
    {
        private static readonly List<GameData> AllData = new(128);
        private static readonly Dictionary<Type, GameData> TypeCache = new(64);

        /// <summary>
        /// Automatically called before the first scene loads.
        /// Loads all GameData inside Resources folders.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            AllData.Clear();
            TypeCache.Clear();

            // Load *all* GameData from any Resources folder
            var loaded = Resources.LoadAll<GameData>("");

            foreach (var data in loaded)
            {
                // Safety: avoid duplicates
                if (AllData.Contains(data))
                    continue;

                AllData.Add(data);

                // Optional: auto-cache if only 1 ScriptableObject exists for that type
                var type = data.GetType();
                TypeCache.TryAdd(type, data);
            }
        }

        /// <summary>
        /// Returns the unique instance of the GameData type T.
        /// First call caches the result. Next calls are O(1).
        /// </summary>
        public static T Get<T>() where T : GameData
        {
            var type = typeof(T);

            // 1) Cache hit → return instantly
            if (TypeCache.TryGetValue(type, out var cached))
                return (T)cached;

            // 2) Search once → add to cache
            foreach (var data in AllData)
            {
                if (data is not T result) continue;
                TypeCache[type] = result;
                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns all GameData of type T (if you have multiple).
        /// Cached internally for fast reuse.
        /// </summary>
        public static IReadOnlyList<T> GetAll<T>() where T : GameData
        {
            var results = ListCache<T>.List;
            results.Clear();

            foreach (var t in AllData)
            {
                if (t is T typed)
                    results.Add(typed);
            }

            return results;
        }

        /// <summary>
        /// Internal static cache of lists per type without allocations.
        /// </summary>
        private static class ListCache<T>
        {
            public static readonly List<T> List = new(32);
        }
    }
}
