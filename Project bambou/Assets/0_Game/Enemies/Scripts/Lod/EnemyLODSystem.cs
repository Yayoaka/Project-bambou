using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Lod
{
    public class EnemyLODSystem : MonoBehaviour
    {
        public static EnemyLODSystem Instance;

        private readonly List<ILODComponent> _lods = new();
        private readonly List<Transform> _players = new();

        [SerializeField] private int updatesPerFrame = 50;
        private int _index;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            UpdateGroup();
        }

        #region Events

        private void OnEnable()
        {
            PlayerCharacterManager.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnDisable()
        {
            PlayerCharacterManager.OnPlayerSpawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(GameObject player)
        {
            _players.Add(player.transform);
        }

        #endregion

        public void Register(ILODComponent c) => _lods.Add(c);
        public void Unregister(ILODComponent c) => _lods.Remove(c);

        void UpdateGroup()
        {
            if (_lods.Count == 0) 
                return;

            var loops = Mathf.Min(updatesPerFrame, _lods.Count);

            for (var i = 0; i < loops; i++)
            {
                if (_index >= _lods.Count)
                    _index = 0;

                var e = _lods[_index];
                var level = CalcLOD(e.Position);
                e.SetLOD(level);

                _index++;
            }
        }

        ILODComponent.LodLevel CalcLOD(Vector3 pos)
        {
            var minDist = float.MaxValue;

            for (var i = 0; i < _players.Count; i++)
            {
                var d = (pos - _players[i].position).sqrMagnitude;
                if (d < minDist)
                    minDist = d;
            }

            if (minDist < 400f) return ILODComponent.LodLevel.High;     // 20m
            if (minDist < 2500f) return ILODComponent.LodLevel.Medium; // 50m

            return ILODComponent.LodLevel.Low;
        }
    }
}