using Character.Visual;
using Skills;
using UnityEngine;

namespace Character.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Characters/CharacterData", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string characterName;
        [SerializeField] private string characterDescription;
        [SerializeField] private Sprite characterIcon;
        [SerializeField] private int baseHealth;
        [SerializeField] private int speed;
        [SerializeField] private CharacterVisual characterVisualPrefab;
        [SerializeField] private SpellData[] spells;

        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public Sprite CharacterIcon => characterIcon;
        public int BaseHealth => baseHealth;
        public int Speed => speed;
        public CharacterVisual CharacterVisualPrefab => characterVisualPrefab;
        public SpellData[] Spells => spells;
    }
}
