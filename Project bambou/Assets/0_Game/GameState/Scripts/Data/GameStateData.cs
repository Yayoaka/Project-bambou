using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameState.Data
{
    [CreateAssetMenu(menuName = "GameState/Game State Data")]
    public class GameStateData : ScriptableObject
    {
        public GameStateType stateType;

        [Header("Scenes to Load")]
        public List<SceneAsset> scenesToLoad;
        public List<SceneAsset> netScenesToLoad;

        [Header("Scenes persistent")]
        public List<SceneAsset> scenesToKeep;
        public List<SceneAsset> netScenesToKeep;
    }
}
