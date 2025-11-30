using Character.Visual;
using Skills;
using UnityEngine;

namespace Character.Data
{
    [System.Serializable]
    public class Stats
    {
        public float health;
        public float abilityDamage;
        public float abilityPower;
        public float armorResistance;
        public float magicResistance;
        public float critChance;
        public float critMultiplier;
        public float moveSpeed;
        public float attackSpeed;
    }
    
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Characters/CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private string characterDescription;
        [SerializeField] private Sprite characterIcon;
        [SerializeField] private CharacterVisual characterVisualPrefab;
        [SerializeField] private SpellData[] spells;
        [SerializeField] private Stats stats;

        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public Sprite CharacterIcon => characterIcon;
        public CharacterVisual CharacterVisualPrefab => characterVisualPrefab;
        public SpellData[] Spells => spells;
        public Stats Stats => stats;
    }
}
