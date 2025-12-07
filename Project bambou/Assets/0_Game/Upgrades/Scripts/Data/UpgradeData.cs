using UnityEngine;

namespace Upgrades.Data
{
    public abstract class UpgradeData : ScriptableObject
    {
        public string Name;
        [Multiline]
        public string Description;
        public Sprite Icon;
    }
}
