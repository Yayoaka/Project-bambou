using UnityEngine;
using UnityEngine.UI;
using Upgrades.Data;

namespace HUD
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text title;
        [SerializeField] private TMPro.TMP_Text description;
        [SerializeField] private Image icon;
        [SerializeField] private Button selectButton;

        private UpgradeData _data;

        public void Setup(UpgradeData data, System.Action onClick)
        {
            _data = data;

            title.text = data.Name;
            description.text = data.Description;
            icon.sprite = data.Icon;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick());
        }
    }
}