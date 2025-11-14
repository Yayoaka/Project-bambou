using Data;
using UnityEngine;

namespace GameState.Data
{
    [CreateAssetMenu(menuName = "GameState/Game State Database")]
    public class GameStateDatabase : GameData
    {
        public GameStateData[] gameStates;
    }
}