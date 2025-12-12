using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class CharacterDataHUD : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private TMPro.TMP_Text healthText;
        
        public void SetHealth(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text = $"{Mathf.RoundToInt(health)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }
}
