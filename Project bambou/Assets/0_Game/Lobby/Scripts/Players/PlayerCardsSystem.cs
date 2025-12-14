using System.Collections.Generic;
using System.Linq;
using Players;
using Unity.Netcode;
using UnityEngine;

namespace Lobby.Players
{
    public class PlayerCardsSystem : MonoBehaviour
    {
        [SerializeField] private PlayerCard playerCardPrefab;
        
        private readonly List<PlayerCard> _playerCards = new();

        private void Start()
        {
            StartCoroutine(InitWhenReady());
        }

        private System.Collections.IEnumerator InitWhenReady()
        {
            while (PlayerDataManager.Instance == null ||
                   !PlayerDataManager.Instance.IsSpawned)
            {
                yield return null;
            }

            var players = PlayerDataManager.Instance.Players;

            SetupPlayersCards(players);
            players.OnListChanged += UpdatePlayersCards;
        }
        
        private void OnDestroy()
        {
            if (PlayerDataManager.Instance != null)
                PlayerDataManager.Instance.Players.OnListChanged -= UpdatePlayersCards;
        }

        
        private void SetupPlayersCards(NetworkList<PlayerData> players)
        {
            foreach (var p in players)
                AddCard(p);
        }
        
        private void UpdatePlayersCards(NetworkListEvent<PlayerData> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Add:
                    AddCard(changeEvent.Value);
                    break;

                case NetworkListEvent<PlayerData>.EventType.Remove:
                    RemoveCard(changeEvent.Value.clientId);
                    break;
                    
                case NetworkListEvent<PlayerData>.EventType.RemoveAt:
                    RemoveCardByIndex(changeEvent.Index);
                    break;

                case NetworkListEvent<PlayerData>.EventType.Value:
                    UpdateCard(changeEvent.Value);
                    break;

                case NetworkListEvent<PlayerData>.EventType.Clear:
                    ClearAll();
                    break;
            }

        }
        
        private void AddCard(PlayerData data)
        {
            if (_playerCards.Exists(c => c.PlayerId == data.clientId))
                return;

            var card = Instantiate(playerCardPrefab, transform);
            card.Setup(data);
            _playerCards.Add(card);
        }

        private void UpdateCard(PlayerData data)
        {
            var card = _playerCards.Find(c => c.PlayerId == data.clientId);
            if (card == null)
                return;

            card.Setup(data);
        }

        private void RemoveCard(ulong clientId)
        {
            var card = _playerCards.First(x => x.PlayerId == clientId);
            _playerCards.Remove(card);
            Destroy(card.gameObject);
        }
        
        private void RemoveCardByIndex(int index)
        {
            if (index < 0 || index >= _playerCards.Count)
                return;

            var card = _playerCards[index];
            _playerCards.RemoveAt(index);
            Destroy(card.gameObject);
        }

        private void ClearAll()
        {
            for (int i = 0; i < _playerCards.Count; i++)
                Destroy(_playerCards[i].gameObject);

            _playerCards.Clear();
        }
    }
}