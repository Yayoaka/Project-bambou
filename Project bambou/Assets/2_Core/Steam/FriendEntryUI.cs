using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam
{
    public class FriendEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button joinButton;

        private CSteamID _friendId;

        public void Setup(CSteamID friendId)
        {
            _friendId = friendId;
            nameText.text = SteamFriends.GetFriendPersonaName(friendId);

            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnJoinClicked()
        {
            SteamLobbyManager.Instance.JoinFriend(_friendId);
        }
    }
}