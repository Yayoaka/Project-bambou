namespace Enemies
{
    public interface IEnemyTick
    {
        // Called by EnemyManager with a centralized dt
        void Tick(float dt);
    }
}
