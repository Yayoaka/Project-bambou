using UnityEngine;

namespace Enemies
{
    public class EnemyManager : MonoBehaviour
    {
        public static int AliveCount { get; private set; }

        public static void RegisterSpawn()
        {
            AliveCount++;
        }

        public static void RegisterDeath()
        {
            AliveCount--;
        }
    }
}