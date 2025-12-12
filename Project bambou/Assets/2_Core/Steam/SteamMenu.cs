using UnityEngine;

namespace Steam
{
    public class SteamMenu : MonoBehaviour
    {
        [SerializeField] private FriendEntryUI friendEntryPrefab;
        [SerializeField] private GameObject friendListContainer;
        
        public void RefreshFriendList()
        {
            foreach (var friend in friendListContainer.GetComponentsInChildren<FriendEntryUI>())
            {
                Destroy(friend.gameObject);
            }
        
            var friends = SteamManager.GetFriendsPlayingThisGame();

            foreach (var friend in friends)
            {
                var entry = Instantiate(friendEntryPrefab, friendListContainer.transform);
            
                entry.Setup(friend);
            }
        }
    }
}
