using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Upgrades.Data;

namespace HUD
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup panel;                // Parent container (activé/désactivé)
        [SerializeField] private Transform cardsParent;           // Où instancier les cartes
        [SerializeField] private UpgradeCardUI cardPrefab;        // Prefab d'une carte

        private Action<UpgradeData> _onSelected;

        private readonly List<UpgradeCardUI> _spawnedCards = new();

        private void Awake()
        {
            if (panel != null)
                panel.alpha = 0;
        }

        // ------------------------------------------------------------------
        // PUBLIC API
        // ------------------------------------------------------------------

        /// <summary>
        /// Affiche plusieurs upgrades (3 le plus souvent).
        /// </summary>
        public void Show(List<UpgradeData> upgrades, int[] levels, Action<UpgradeData> onSelected)
        {
            ClearCards();

            _onSelected = onSelected;
            panel.alpha = 1;
            panel.blocksRaycasts = true;
            panel.interactable = true;

            for (var i = 0; i < upgrades.Count; i++)
            {
                CreateCard(upgrades[i], levels[i]);
            }
        }

        /// <summary>
        /// Affiche un fallback si aucune upgrade n'est disponible.
        /// </summary>
        public void ShowFallback()
        {
            ClearCards();

            panel.alpha = 1;
        }

        /// <summary>
        /// Ferme et nettoie.
        /// </summary>
        public void Hide()
        {
            panel.alpha = 0;
            panel.blocksRaycasts = false;
            panel.interactable = false;
            
            ClearCards();
        }

        // ------------------------------------------------------------------
        // INTERNAL
        // ------------------------------------------------------------------

        private void CreateCard(UpgradeData data, int lvl)
        {
            var card = Instantiate(cardPrefab, cardsParent);
            card.Setup(data, lvl, () => OnCardClicked(data));
            _spawnedCards.Add(card);
        }

        private void OnCardClicked(UpgradeData data)
        {
            panel.alpha = 0;

            _onSelected?.Invoke(data);
            _onSelected = null;
        }

        private void ClearCards()
        {
            foreach (var c in _spawnedCards)
            {
                if (c != null)
                    Destroy(c.gameObject);
            }
            _spawnedCards.Clear();
        }
    }
}
