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
        
        private readonly List<SceneAsset> _currentLocalScenes = new();
        private readonly List<SceneAsset> _currentNetScenes = new();
        
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

            foreach (var scene in localToUnload)
            {
                yield return UnloadLocalScene(scene);
            }

            var scenesToKeepNet = state.netScenesToKeep;
            var desiredNet = state.netScenesToLoad;

            var netToUnload = _currentNetScenes
                .Where(s => !desiredNet.Contains(s) && !scenesToKeepNet.Contains(s))
                .ToList();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var scene in netToUnload)
                {
                    yield return UnloadNetScene(scene);
                }
            }

            foreach (var sceneAsset in state.scenesToLoad.Where(sceneAsset => !_currentLocalScenes.Contains(sceneAsset)))
            {
                yield return LoadLocalScene(sceneAsset);
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var sceneAsset in state.netScenesToLoad.Where(sceneAsset => !_currentNetScenes.Contains(sceneAsset)))
                {
                    yield return LoadNetScene(sceneAsset);
                }
            }

            OnScenesLoaded?.Invoke();
        }

        #endregion

        #region Local Scene Loading

        IEnumerator LoadLocalScene(SceneAsset scene)
        {
            var op = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);

            if (op == null)
                yield break;

            while (!op.isDone)
                yield return null;

            if (!_currentLocalScenes.Contains(scene))
                _currentLocalScenes.Add(scene);
        }

        IEnumerator UnloadLocalScene(SceneAsset scene)
        {
            var op = SceneManager.UnloadSceneAsync(scene.name);

            while (!op.isDone)
                yield return null;

            _currentLocalScenes.Remove(scene);
            OnScenesUnloaded?.Invoke();
        }

        #endregion

        #region Netcode Scene Loading

        private IEnumerator LoadNetScene(SceneAsset scene)
        {
            var done = false;
            
            Debug.Log($"Loading scene {scene.name}");

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoaded;

            NetworkManager.Singleton.SceneManager.LoadScene(scene.name, LoadSceneMode.Additive);

            while (!done)
                yield return null;

            if (!_currentNetScenes.Contains(scene))
                _currentNetScenes.Add(scene);
            yield break;

            void OnLoaded(
                string loadedScene,
                LoadSceneMode mode,
                List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (scene.name != loadedScene) return;

                done = true;

                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoaded;
            }
        }

        private IEnumerator UnloadNetScene(SceneAsset sceneAsset)
        {
            var done = false;

            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnUnloaded;

            var scene = SceneManager.GetSceneByName(sceneAsset.name);
            
            NetworkManager.Singleton.SceneManager.UnloadScene(scene);

            while (!done)
                yield return null;

            _currentNetScenes.Remove(sceneAsset);
            OnScenesUnloaded?.Invoke();
            yield break;

            void OnUnloaded(string unloadedScene, LoadSceneMode mode, List<ulong> clientsCompleted,
                List<ulong> clientsTimedOut)
            {
                if (unloadedScene != sceneAsset.name)
                    return;

                done = true;
                NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted -= OnUnloaded;
            }
        }

        #endregion
    }
}
