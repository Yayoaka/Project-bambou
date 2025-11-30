using Unity.Netcode;

namespace Entity
{
    public abstract class EntityBehaviour<TSelf> : NetworkBehaviour
        where TSelf : EntityBehaviour<TSelf>
    {
        protected T InitComponent<T>() where T : EntityComponent<TSelf>
        {
            var c = GetComponent<T>();
            c.Init((TSelf)this);
            return c;
        }
    }
}