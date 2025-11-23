using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Skills
{
    public class Zone : NetworkBehaviour
    {
        private float _duration;
        private int _damages = 10;
        
        public void Init(EffectData data)
        {
            _duration = data.duration;
            StartCoroutine(DestroyCoroutine());
        }
        
        private IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(_duration);
            GetComponent<NetworkObject>().Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if(!other.CompareTag("Enemy")) return;
            
            other.GetComponent<IEnemy>().Damage(_damages);
        }
    }
}