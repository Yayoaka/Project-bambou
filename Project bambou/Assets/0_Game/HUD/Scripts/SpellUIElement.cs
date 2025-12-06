using Skills.Data;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class SpellUIElement : MonoBehaviour
    {
        [SerializeField] private Image spellIcon;
        public void SetSpell(SpellData spell)
        {
            spellIcon.sprite = spell.spellIcon;
        }
    }
}