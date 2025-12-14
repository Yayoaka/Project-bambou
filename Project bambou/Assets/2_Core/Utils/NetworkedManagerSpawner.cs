using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class NetworkedManagerSpawner : MonoBehaviour
    {
        [SerializeField] private List<NetworkObject> managers = new List<NetworkObject>();

        private void Awake()
        {
            foreach (var manager in managers)
            {
                var go = Instantiate(manager);
                go.Spawn();
            }
        }
    }
}