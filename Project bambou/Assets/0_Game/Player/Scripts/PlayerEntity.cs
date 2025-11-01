using UnityEngine;

namespace Player
{
    //TODO: Rework that class (actually it's just to register)
    public class PlayerEntity : MonoBehaviour
    {
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            Enemies.EnemyManager.Instance.RegisterPlayer(transform);
        }
    }
}
