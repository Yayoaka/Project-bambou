namespace GameState
{
    public enum GameStateType
    {
        None,
        Menu,
        Lobby,
        Loading,
        Mission,
        GameOver
    }

    public interface IGameStateContext { }
    
    public interface IGameState
    {
        protected internal void Enter(IGameStateContext ctx);
        protected internal void Exit();
        protected internal void Tick(float dt);

        protected bool CanPause();
        
        protected void OnPause();
        
        protected void OnResume();
    }
}
