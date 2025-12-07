using UnityEngine;
using UnityEngine.UI;
using Upgrades.Data;

namespace HUD
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text title;
        [SerializeField] private TMPro.TMP_Text description;
        [SerializeField] private TMPro.TMP_Text lvlText;
        [SerializeField] private Image icon;
        [SerializeField] private Button selectButton;

        private UpgradeData _data;

        public void Setup(UpgradeData data, int lvl, System.Action onClick)
        {
            _data = data;

            title.text = data.Name;
            description.text = data.Description;
            icon.sprite = data.Icon;
            lvlText.text = "Lvl " + (lvl + 1);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick());
        }
    }
}