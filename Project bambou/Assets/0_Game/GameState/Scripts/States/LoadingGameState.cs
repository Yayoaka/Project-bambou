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
            
            //Use a loading screen manager to show it
            
            SceneLoader.SceneLoaderManager.OnScenesLoaded += OnSceneLoaded;
        }

        void IGameState.Exit()
        {
            //Use a loading screen manager to hide it
            
            SceneLoader.SceneLoaderManager.OnScenesLoaded -= OnSceneLoaded;
        }

        void IGameState.Tick(float dt)
        {
            //Update loading percentage display
        }

        bool IGameState.CanPause()
        {
            return false;
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
            if(_context is not LoadingContext loadingContext) return;
            
            GameStateManager.Instance.ChangeState(loadingContext.TargetGameState);
        }
    }
}