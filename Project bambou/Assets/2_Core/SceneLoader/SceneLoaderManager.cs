using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using GameState;
using GameState.Data;
using GameState.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoader
{
    public class SceneLoaderManager : MonoBehaviour
    {
        #region Singleton

        public static SceneLoaderManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSceneAsync(new LoadingContext(defaultState));
        }

        #endregion

        #region Events

        public static event Action OnScenesLoaded;
        public static event Action OnScenesUnloaded;

        #endregion

        #region Private Variables

        private readonly List<string> _currentLocalScenes = new();
        private readonly List<string> _currentNetScenes = new();

        [SerializeField] private GameStateType defaultState;

        #endregion

        #region SceneLoad

        public void LoadSceneAsync(LoadingContext ctx)
        {
            var database = GameDatabase.Get<GameStateDatabase>();
            var stateData = database.gameStates.First(x => x.stateType == ctx.TargetGameState);

            GameStateManager.Instance.ChangeState(GameStateType.Loading, ctx);

            StartCoroutine(LoadRoutine(stateData));
        }

        #endregion

        #region Load Routine

        private IEnumerator LoadRoutine(GameStateData state)
        {
            var scenesToKeepLocal = state.scenesToKeep;
            var desiredLocal = state.scenesToLoad;

            var localToUnload = _currentLocalScenes
                .Where(s => !desiredLocal.Contains(s) && !scenesToKeepLocal.Contains(s))
                .ToList();

            foreach (var sceneName in localToUnload)
                yield return UnloadLocalScene(sceneName);

            var scenesToKeepNet = state.netScenesToKeep;
            var desiredNet = state.netScenesToLoad;

            var netToUnload = _currentNetScenes
                .Where(s => !desiredNet.Contains(s) && !scenesToKeepNet.Contains(s))
                .ToList();

            var nm = NetworkManager.Singleton;
            var isNetActive = nm != null && nm.IsListening;
            var isServer = isNetActive && nm.IsServer;
            var isClient = isNetActive && nm.IsClient;

            // SERVER: drive Netcode scene unloads.
            if (isServer)
            {
                foreach (var sceneName in netToUnload)
                    yield return UnloadNetScene(sceneName);
            }

            // LOCAL scenes (client + server)
            foreach (var sceneName in desiredLocal.Where(scene => !_currentLocalScenes.Contains(scene)))
                yield return LoadLocalScene(sceneName);

            // SERVER: drive Netcode scene loads.
            if (isServer)
            {
                foreach (var sceneName in desiredNet.Where(scene => !_currentNetScenes.Contains(scene)))
                    yield return LoadNetScene(sceneName);
            }

            // CLIENT: DO NOT load/unload net scenes, just WAIT for Netcode sync.
            // This is the fix for "join host => locals change but net scenes don't appear".
            if (isClient && !isServer)
            {
                if (netToUnload.Count > 0)
                    yield return WaitForNetScenesUnloaded(netToUnload);

                if (desiredNet.Count > 0)
                    yield return WaitForNetScenesLoaded(desiredNet);
            }

            OnScenesLoaded?.Invoke();
        }

        #endregion

        #region Local Scene Loading

        private IEnumerator LoadLocalScene(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null)
                yield break;

            while (!op.isDone)
                yield return null;

            if (!_currentLocalScenes.Contains(sceneName))
                _currentLocalScenes.Add(sceneName);
        }

        private IEnumerator UnloadLocalScene(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
                yield break;

            while (!op.isDone)
                yield return null;

            _currentLocalScenes.Remove(sceneName);
            OnScenesUnloaded?.Invoke();
        }

        #endregion

        #region Netcode Scene Loading (SERVER ONLY)

        private IEnumerator LoadNetScene(string sceneName)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer)
                yield break;

            var done = false;

            Debug.Log($"[NetScene] Loading {sceneName}");

            void OnLoaded(
                string loadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (loadedScene != sceneName)
                    return;

                done = true;
                nm.SceneManager.OnLoadEventCompleted -= OnLoaded;

                if (!_currentNetScenes.Contains(sceneName))
                    _currentNetScenes.Add(sceneName);

                if (clientsTimedOut.Count > 0)
                    Debug.LogWarning($"[NetScene] Load timed out for {clientsTimedOut.Count} client(s) on scene {sceneName}.");
            }

            nm.SceneManager.OnLoadEventCompleted += OnLoaded;
            nm.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            while (!done)
                yield return null;
        }

        private IEnumerator UnloadNetScene(string sceneName)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsServer)
                yield break;

            var done = false;

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                _currentNetScenes.Remove(sceneName);
                yield break;
            }

            void OnUnloaded(
                string unloadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (unloadedScene != sceneName)
                    return;

                done = true;
                nm.SceneManager.OnUnloadEventCompleted -= OnUnloaded;

                _currentNetScenes.Remove(sceneName);

                if (clientsTimedOut.Count > 0)
                    Debug.LogWarning($"[NetScene] Unload timed out for {clientsTimedOut.Count} client(s) on scene {sceneName}.");
            }

            nm.SceneManager.OnUnloadEventCompleted += OnUnloaded;
            nm.SceneManager.UnloadScene(scene);

            while (!done)
                yield return null;

            OnScenesUnloaded?.Invoke();
        }

        #endregion

        #region Netcode Sync Wait (CLIENT ONLY)

        private IEnumerator WaitForNetScenesLoaded(List<string> desiredNetScenes)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsClient)
                yield break;

            var pending = new HashSet<string>();

            for (var i = 0; i < desiredNetScenes.Count; i++)
            {
                var sceneName = desiredNetScenes[i];
                var scene = SceneManager.GetSceneByName(sceneName);

                if (scene.IsValid() && scene.isLoaded)
                {
                    if (!_currentNetScenes.Contains(sceneName))
                        _currentNetScenes.Add(sceneName);
                    continue;
                }

                pending.Add(sceneName);
            }

            if (pending.Count == 0)
                yield break;

            void OnLoaded(
                string loadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (!pending.Contains(loadedScene))
                    return;

                pending.Remove(loadedScene);

                if (!_currentNetScenes.Contains(loadedScene))
                    _currentNetScenes.Add(loadedScene);
            }

            nm.SceneManager.OnLoadEventCompleted += OnLoaded;

            while (pending.Count > 0)
                yield return null;

            nm.SceneManager.OnLoadEventCompleted -= OnLoaded;
        }

        private IEnumerator WaitForNetScenesUnloaded(List<string> netScenesToUnload)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsClient)
                yield break;

            var pending = new HashSet<string>();

            for (var i = 0; i < netScenesToUnload.Count; i++)
            {
                var sceneName = netScenesToUnload[i];
                var scene = SceneManager.GetSceneByName(sceneName);

                if (!scene.IsValid() || !scene.isLoaded)
                {
                    _currentNetScenes.Remove(sceneName);
                    continue;
                }

                pending.Add(sceneName);
            }

            if (pending.Count == 0)
                yield break;

            void OnUnloaded(
                string unloadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (!pending.Contains(unloadedScene))
                    return;

                pending.Remove(unloadedScene);
                _currentNetScenes.Remove(unloadedScene);
            }

            nm.SceneManager.OnUnloadEventCompleted += OnUnloaded;

            while (pending.Count > 0)
                yield return null;

            nm.SceneManager.OnUnloadEventCompleted -= OnUnloaded;
        }

        #endregion
    }
}
