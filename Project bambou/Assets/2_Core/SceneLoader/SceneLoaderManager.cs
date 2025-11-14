using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using GameState;
using GameState.Data;
using GameState.States;
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

        #region Serialized Variables

        [SerializeField] private GameStateType defaultState;

        #endregion
        
        #region Private Variables
        
        private readonly List<SceneAsset> _currentScenes = new();

        #endregion
        
        #region Scene Load
        
        public void LoadSceneAsync(LoadingContext context)
        {
            GameStateManager.Instance.ChangeState(GameStateType.Loading, context);

            StartCoroutine(LoadRoutine(
                GameDatabase.Get<GameStateDatabase>().gameStates.First(x => x.stateType == context.TargetGameState)));
        }
        
        #endregion
        
        #region Load Coroutines

        IEnumerator LoadRoutine(GameStateData groupData)
        {
            var remainScenes = _currentScenes;
            var operations = new List<AsyncOperation>();
            
            foreach (var scene in groupData.scenesToLoad)
            {
                if (remainScenes.Contains(scene))
                { 
                    remainScenes.Remove(scene);
                    continue;
                }
                
                var op = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);

                if (op == null) continue;
                
                operations.Add(op);
                    
                op.allowSceneActivation = false;

                // Wait until 90% (Unity loads scene but waits activation)
                while (op.progress < 0.9f)
                    yield return null;
            }
            
            var scenesToUnload = remainScenes.Where(x => !groupData.scenesToKeep.Contains(x));

            foreach (var scene in scenesToUnload)
            {
                yield return UnloadRoutine(scene.name);
            }

            foreach (var op in operations)
            {
                op.allowSceneActivation = true;
            }

            while (operations.All(x => x.isDone != true))
            {
                yield return null;
            }
            
            OnScenesLoaded?.Invoke();
        }

        IEnumerator UnloadRoutine(string sceneName)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);

            while (!op.isDone)
                yield return null;

            OnScenesUnloaded?.Invoke();
        }
        
        #endregion
    }
}
