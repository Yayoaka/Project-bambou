using Experience;
using UnityEngine;

namespace Collectibles
{
    public class XpCollectible : MagneticCollectible
    {
        private int _amount = 1;

        public void Init(int amount = 1)
        {
            base.Init();
            _amount = amount;
        }
        
        protected override void OnCollectedServer()
        {
            SharedExperienceManager.Instance.AddXP(_amount);
        }
    }
}