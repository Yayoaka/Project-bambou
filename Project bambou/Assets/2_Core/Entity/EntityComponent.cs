using Unity.Netcode;

namespace Entity
{
    public abstract class EntityComponent<TBehaviour> : NetworkBehaviour
        where TBehaviour : EntityBehaviour<TBehaviour>
    {
        protected TBehaviour Owner { get; private set; }

        public virtual void Init(TBehaviour owner)
        {
            Owner = owner;
        }

        public virtual void LateInit(){}
    }
}