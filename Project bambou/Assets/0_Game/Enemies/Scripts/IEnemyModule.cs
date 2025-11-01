namespace Enemies
{
    public interface IEnemyModule
    {
        // Called once when the Enemy composes its modules
        void Setup(EnemyContext ctx);

        // Called when pulling from pool / enabling
        void OnActivated();

        // Called when returning to pool / disabling
        void OnDeactivated();
    }
}
