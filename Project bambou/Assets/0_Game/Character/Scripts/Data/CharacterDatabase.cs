using System.Linq;
using Data;
using UnityEngine;

namespace Character.Data
{
    [CreateAssetMenu(menuName = "Characters/Characters Database")]
    public class CharacterDatabase : GameData
    {
        [SerializeField] private CharacterData[] characters;
        
        public CharacterData[] Characters => characters;

        public CharacterData GetCharacter(string id) => characters.FirstOrDefault(x => x.CharacterName == id);
    }
}
