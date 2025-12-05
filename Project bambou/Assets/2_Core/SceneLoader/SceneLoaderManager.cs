using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using GameState;
using GameState.Data;
using GameState.States;
using Unity.Netcode;
using UnityEditor;
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
            {
                yield return UnloadLocalScene(sceneName);
            }

            var scenesToKeepNet = state.netScenesToKeep;
            var desiredNet = state.netScenesToLoad;

            var netToUnload = _currentNetScenes
                .Where(s => !desiredNet.Contains(s) && !scenesToKeepNet.Contains(s))
                .ToList();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var sceneName in netToUnload)
                {
                    yield return UnloadNetScene(sceneName);
                }
            }

            foreach (var sceneName in desiredLocal.Where(scene => !_currentLocalScenes.Contains(scene)))
            {
                yield return LoadLocalScene(sceneName);
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var sceneName in desiredNet.Where(sceneAsset => !_currentNetScenes.Contains(sceneAsset)))
                {
                    yield return LoadNetScene(sceneName);
                }
            }

            OnScenesLoaded?.Invoke();
        }

        #endregion

        #region Local Scene Loading

        IEnumerator LoadLocalScene(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (op == null)
                yield break;

            while (!op.isDone)
                yield return null;

            if (!_currentLocalScenes.Contains(sceneName))
                _currentLocalScenes.Add(sceneName);
        }

        IEnumerator UnloadLocalScene(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);

            while (!op.isDone)
                yield return null;

            _currentLocalScenes.Remove(sceneName);
            OnScenesUnloaded?.Invoke();
        }

        #endregion

        #region Netcode Scene Loading

        private IEnumerator LoadNetScene(string sceneName)
        {
            var done = false;
            
            Debug.Log($"Loading scene {sceneName}");

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoaded;

            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            while (!done)
                yield return null;

            if (!_currentNetScenes.Contains(sceneName))
                _currentNetScenes.Add(sceneName);
            yield break;

            void OnLoaded(
                string loadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (sceneName != loadedScene) return;

                done = true;

                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoaded;
            }
        }

        private IEnumerator UnloadNetScene(string sceneName)
        {
            var done = false;

            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnUnloaded;

            var scene = SceneManager.GetSceneByName(sceneName);
            
            NetworkManager.Singleton.SceneManager.UnloadScene(scene);

            while (!done)
                yield return null;

            _currentNetScenes.Remove(sceneName);
            OnScenesUnloaded?.Invoke();
            yield break;

            void OnUnloaded(string unloadedScene, LoadSceneMode mode, List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (unloadedScene != sceneName)
                    return;

                done = true;
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloaded;
            }
        }

        #endregion
    }
}
