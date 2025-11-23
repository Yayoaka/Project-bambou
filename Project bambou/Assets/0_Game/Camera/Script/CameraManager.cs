using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CameraManager : MonoBehaviour
    {
        private static CinemachineCamera _camera;
        
        [SerializeField] private new CinemachineCamera camera;

        public void Awake()
        {
            _camera = camera;
        }

        public static void SetTarget(Transform target)
        {
            _camera.Follow = target;
        }
    }
}