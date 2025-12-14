using Enemies;
using Network;
using Networking;

namespace GameState
{
    public class MissionGameState : IGameState
    {
        void IGameState.Enter(IGameStateContext ctx)
        {
            //throw new System.NotImplementedException();
        }

        void IGameState.Exit()
        {
            NetworkObjectPool.Instance.ClearPooledObjects();
            
            PlayerCharacterManager.instance.ClearPlayers();
        }

        void IGameState.Tick(float dt)
        {
            //throw new System.NotImplementedException();
        }

        bool IGameState.CanPause()
        {
            throw new System.NotImplementedException();
        }

        void IGameState.OnPause()
        {
            throw new System.NotImplementedException();
        }

        void IGameState.OnResume()
        {
            throw new System.NotImplementedException();
        }
    }
}