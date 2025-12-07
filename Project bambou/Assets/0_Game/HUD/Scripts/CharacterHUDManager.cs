using System;
using System.Collections.Generic;
using Skills.Data;
using UnityEngine;
using Upgrades.Data;

namespace HUD
{
    public class CharacterHUDManager : MonoBehaviour
    {
        #region Singleton

        public static CharacterHUDManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        #region Spell UI

        [SerializeField] private SpellUIElement[] spellUIElements;

        public void SetSpells(SpellData[] spells)
        {
            if (spells == null || spellUIElements == null)
            {
                Debug.LogError("[HUD] Spells or UI elements missing.");
                return;
            }

            if (spells.Length != spellUIElements.Length)
            {
                Debug.LogWarning("[HUD] Spell count does not match UI element count. Will assign what I can.");
            }

            for (int i = 0; i < spellUIElements.Length; i++)
            {
                if (i < spells.Length)
                    spellUIElements[i].SetSpell(spells[i]);
            }
        }

        #endregion
        
        #region Upgrade UI

        [SerializeField] private UpgradeSelectionUI upgradeSelectionUI;

        /// <summary>
        /// Affiche une sélection de plusieurs upgrades (mode 3 choix).
        /// </summary>
        public void ShowUpgradeChoices(
            List<UpgradeData> upgrades,
            int[] levels,
            Action<UpgradeData> onSelected)
        {
            if (upgradeSelectionUI == null)
            {
                Debug.LogError("[HUD] Missing UpgradeSelectionUI reference.");
                return;
            }

            upgradeSelectionUI.Show(upgrades, levels, onSelected);
        }

        /// <summary>
        /// Affiche une seule carte d’upgrade (upgrade forcée).
        /// </summary>
        public void ShowSingleUpgrade(UpgradeData upgrade, int level, Action<UpgradeData> onSelected)
        {
            if (upgradeSelectionUI == null)
            {
                Debug.LogError("[HUD] Missing UpgradeSelectionUI reference.");
                return;
            }

            upgradeSelectionUI.ShowSingle(upgrade, level, onSelected);
        }

        /// <summary>
        /// Affiche un fallback quand aucune upgrade n’est éligible.
        /// </summary>
        public void ShowSingleUpgradeFallback()
        {
            if (upgradeSelectionUI == null)
            {
                Debug.LogError("[HUD] Missing UpgradeSelectionUI reference.");
                return;
            }

            upgradeSelectionUI.ShowFallback();
        }

        #endregion
    }
}
