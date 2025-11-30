using System.Linq;
using Data;
using UnityEngine;

namespace Enemies.Data
{
    [CreateAssetMenu(menuName = "Enemies/Enemies Database")]
    public class EnemyDatabase : GameData
    {
        [SerializeField] private EnemyDataSo[] enemies;
        
        public EnemyDataSo[] Enemies => enemies;

        public EnemyDataSo GetEnemy(string id) => enemies.FirstOrDefault(x => x.id == id);
    }
}