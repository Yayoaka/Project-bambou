using Unity.Netcode;
using UnityEngine;
using Upgrades;

namespace Collectibles
{
    public class CollectibleUpgrade : MagneticCollectible
    {
        protected override void OnCollectedServer()
        {
            UpgradesManager.Instance.CallSingleUpgrade();
        }
    }
}