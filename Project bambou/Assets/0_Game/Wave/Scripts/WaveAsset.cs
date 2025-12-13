using System;
using System.Collections.Generic;
using Enemies.Data;
using GameState;
using UnityEngine;
using Wave.Patterns;

namespace Wave
{
    public enum WaveActionType
    {
        Spawn,
        WaitTime,
        WaitUntilClear,
        WaitUntilAliveCount
    }
    
    public enum WaveCompletionAction
    {
        None,
        NextWave,
        ChangeGameState
    }

    [CreateAssetMenu(menuName = "Waves/Wave")]
    public class WaveAsset : ScriptableObject
    {
        public List<WaveAction> actions = new();
        
        [Header("On Complete")]
        public WaveCompletionAction onComplete;
        public WaveAsset nextWave;
        public GameStateType nextGameState;
    }

    [Serializable]
    public struct WaveAction
    {
        public WaveActionType type;

        // Spawn
        public EnemyDataSo enemy;
        public int count;
        public float spawnInterval;
        public float startDelay;
        public SpawnPattern pattern;

        // Wait
        public float duration;
        public int aliveCount;
    }
}