using System.Collections.Generic;
using Effect;
using UnityEngine;
using Upgrades.Data;

namespace Upgrades.EffectUpgrades.Data
{
    [CreateAssetMenu(menuName = "Upgrades/EffectUpgrade", fileName = "EffectUpgrade")]
    public class EffectUpgradeData : UpgradeData
    {
        public List<EffectData> Effects = new();
    }
}