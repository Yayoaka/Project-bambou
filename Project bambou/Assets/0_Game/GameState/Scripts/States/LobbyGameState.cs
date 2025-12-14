namespace GameState.States
{
    public struct LobbyContext : IGameStateContext { }
    
    public class LobbyGameState : IGameState
    {
        void IGameState.Enter(IGameStateContext ctx)
        {
            //throw new System.NotImplementedException();
        }

        void IGameState.Exit()
        {
            
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