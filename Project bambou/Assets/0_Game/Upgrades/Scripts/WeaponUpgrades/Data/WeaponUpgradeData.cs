using System.Collections.Generic;
using Effect;
using UnityEngine;
using Upgrades.Data;
using Upgrades.PassiveUpgrades.Data;

namespace Upgrades.WeaponUpgrades.Data
{
    [CreateAssetMenu(menuName = "Upgrades/WeaponUpgrade", fileName = "WeaponUpgrade")]
    public class WeaponUpgradeData : UpgradeData
    {
        public float Cooldown = 0f;
        
        public List<EffectCastData> WeaponEffects;

        public PassiveUpgradeData synergy;
    }
}