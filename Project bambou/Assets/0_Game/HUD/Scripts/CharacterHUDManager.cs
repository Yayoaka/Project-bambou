using System.Collections.Generic;
using Skills.Data;
using UnityEngine;

namespace HUD
{
    public class CharacterHUDManager : MonoBehaviour
    {
        #region Singleton

        public static CharacterHUDManager instance;

        private void Awake()
        {
            instance = this;
        }

        #endregion

        #region Variables

        [SerializeField] private SpellUIElement[] spellUIElement;

        #endregion

        public void SetSpells(SpellData[] spells)
        {
            for (int i = 0; i < spellUIElement.Length; i++)
            {
                spellUIElement[i].SetSpell(spells[i]);
            }
        }
    }
}
