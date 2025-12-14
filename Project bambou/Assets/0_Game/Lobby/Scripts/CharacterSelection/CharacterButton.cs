using Character.Data;
using DG.Tweening;
using Players;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.CharacterSelection
{
    public class CharacterButton : MonoBehaviour
    {
        private static CharacterButton _selectedButton;
        
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image lockedImage;
        [SerializeField] private Image selectedImage;
        [SerializeField] private Image highlightedImage;
        [SerializeField] private Button button;
        
        public void Setup(CharacterData characterData)
        {
            buttonImage.sprite = characterData.CharacterIcon;

            button.onClick.AddListener(Highlight);
            button.onClick.AddListener(() =>
            {
                PlayerDataManager.Instance.SetCharacterServerRpc(
                    new FixedString32Bytes(characterData.CharacterName)
                );
            });
        }
        
        private void Highlight()
        {
            if(_selectedButton == this)  return;
            
            if (_selectedButton) _selectedButton.Deselect();
            
            _selectedButton = this;
            
            highlightedImage.DOFade(1, 0.1f).Play();
        }

        private void Deselect()
        {
            highlightedImage.DOFade(0, 0.1f).Play();
        }
    }
}