using Character.Visual;
using Skills;
using Skills.Data;
using Stats.Data;
using UnityEngine;

namespace Character.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Characters/CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private string characterDescription;
        [SerializeField] private Sprite characterIcon;
        [SerializeField] private Sprite characterRoleIcon; //TEMP
        [SerializeField] private CharacterVisual characterVisualPrefab;
        [SerializeField] private SpellData[] spells;
        [SerializeField] private StatsData stats;

        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public Sprite CharacterIcon => characterIcon;
        public Sprite CharacterRoleIcon => characterRoleIcon;
        public CharacterVisual CharacterVisualPrefab => characterVisualPrefab;
        public SpellData[] Spells => spells;
        public StatsData Stats => stats;
    }
}
