using System.Linq;
using Character.Data;
using Data;
using Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Players
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField] private Image characterImage;
        [SerializeField] private Image roleImage;
        [SerializeField] private TMP_Text playerNameText;

        private ulong _playerId;
        
        public ulong PlayerId => _playerId;
        
        public void Setup(PlayerData playerData)
        {
            _playerId = playerData.clientId;
            playerNameText.text = playerData.steamName.ToString();

            if (playerData.characterId != default)
            {
                SetCharacter(playerData.characterId.ToString());
            }
        }
        
        private void SetCharacter(string characterId)
        {
            var character = GameDatabase.Get<CharacterDatabase>().Characters.First(x => x.CharacterName == characterId);
            characterImage.sprite = character.CharacterIcon;
            roleImage.sprite = character.CharacterRoleIcon;
        }
    }
}