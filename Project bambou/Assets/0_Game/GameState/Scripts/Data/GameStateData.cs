using System.Collections.Generic;
using SceneLoader;
using UnityEngine;

namespace GameState.Data
{
    [CreateAssetMenu(menuName = "GameState/Game State Data")]
    public class GameStateData : ScriptableObject
    {
        public GameStateType stateType;

        [Header("Scenes to Load")]
        public List<string> scenesToLoad;
        public List<string> netScenesToLoad;

        [Header("Scenes persistent")]
        public List<string> scenesToKeep;
        public List<string> netScenesToKeep;
    }
}