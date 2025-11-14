namespace GameState.States
{
    public struct LoadingContext : IGameStateContext
    {
        public readonly GameStateType TargetGameState;

        public LoadingContext(GameStateType targetGameState)
        {
            TargetGameState = targetGameState;
        }
    }
    
    public class LoadingGameState : IGameState
    {
        private IGameStateContext _context;
        
        void IGameState.Enter(IGameStateContext ctx)
        {
            _context = ctx;
            
            SceneLoader.SceneLoaderManager.OnScenesLoaded += OnSceneLoaded;
        }

        void IGameState.Exit()
        {
            
        }

        void IGameState.Tick(float dt)
        {
            
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

        private void OnSceneLoaded()
        {
            SceneLoader.SceneLoaderManager.OnScenesLoaded -= OnSceneLoaded;
            
            if(_context is not LoadingContext loadingContext) return;
            
            GameStateManager.Instance.ChangeState(loadingContext.TargetGameState);
        }
    }
}