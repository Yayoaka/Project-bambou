using System.Collections.Generic;
using Effect;
using UnityEngine;
using Upgrades.Data;

namespace Upgrades.PassiveUpgrades.Data
{
    [CreateAssetMenu(menuName = "Upgrades/PassiveUpgrade", fileName = "PassiveUpgrade")]
    public class PassiveUpgradeData : UpgradeData
    {
        public List<EffectData> PassiveEffects = new();
    }
}
