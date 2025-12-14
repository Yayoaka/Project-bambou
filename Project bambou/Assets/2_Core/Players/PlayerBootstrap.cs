using System.Collections;
using Steam;
using Unity.Netcode;

namespace Players
{
    public class PlayerBootstrap : NetworkBehaviour
    {
        private bool _registered;

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
                return;

            StartCoroutine(RegisterWhenReady());
        }

        private IEnumerator RegisterWhenReady()
        {
            while (PlayerDataManager.Instance == null ||
                   !PlayerDataManager.Instance.IsSpawned)
            {
                yield return null;
            }

            if (_registered)
                yield break;

            _registered = true;

            var steamName = SteamUtilsWrapper.GetLocalSteamName();
            var steamId = SteamUtilsWrapper.GetLocalSteamId();

            PlayerDataManager.Instance.RegisterLocalPlayerServerRpc(
                steamId,
                steamName
            );
        }
    }
}