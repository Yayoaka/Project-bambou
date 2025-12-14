using Character.Data;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.CharacterSelection
{
    public class CharactersSelectionSystem : MonoBehaviour
    {
        [SerializeField] private CharacterButton characterButtonPrefab;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            foreach (var character in GameDatabase.Get<CharacterDatabase>().Characters)
            {
                var button = Instantiate(characterButtonPrefab, transform);
                
                button.Setup(character);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }
}