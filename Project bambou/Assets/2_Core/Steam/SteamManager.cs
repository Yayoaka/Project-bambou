using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Steam
{
    public class SteamManager : MonoBehaviour
    {
        private static bool _initialized = false;
        public static bool Initialized => _initialized;
        
        private void Awake()
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("SteamAPI init failed OUVRE STEAM OU JE TE BUTE (on rigole biensur");
                Application.Quit();
                return;
            }

            _initialized = true;
        }

        private void Update()
        {
            if (SteamAPI.IsSteamRunning())
                SteamAPI.RunCallbacks();
        }

        private void OnDestroy()
        {
            if (SteamAPI.IsSteamRunning())
                SteamAPI.Shutdown();
        }
        
        public static List<CSteamID> GetFriendsPlayingThisGame()
        {
            var friends = new List<CSteamID>();
            var friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

            for (var i = 0; i < friendCount; i++)
            {
                var friendId = SteamFriends.GetFriendByIndex(
                    i,
                    EFriendFlags.k_EFriendFlagImmediate
                );

                if (SteamFriends.GetFriendGamePlayed(friendId, out var gameInfo))
                {
                    if (gameInfo.m_gameID.AppID() == SteamUtils.GetAppID())
                    {
                        friends.Add(friendId);
                    }
                }
            }

            return friends;
        }
    }
}