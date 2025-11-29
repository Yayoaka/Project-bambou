using Health;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Enemies
{
    public class Enemy : NetworkBehaviour, IAffectable
    {
        [SerializeField] private HealthComponent healthComponent;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
        }
        
        public void Damage(HealthEventData healthEventData)
        {
            healthComponent.ApplyDamage(healthEventData);
        }
    }
}