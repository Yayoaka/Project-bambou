using Steamworks;

namespace Steam
{
    public static class SteamUtilsWrapper
    {
        public static string GetLocalSteamName()
        {
            if (!SteamManager.Initialized)
                return "Unknown";

            return SteamFriends.GetPersonaName();
        }

        public static ulong GetLocalSteamId()
        {
            return SteamUser.GetSteamID().m_SteamID;
        }
    }
}