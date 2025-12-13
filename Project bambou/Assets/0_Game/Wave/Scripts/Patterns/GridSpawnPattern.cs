using System.Collections.Generic;
using Enemies.Data;
using UnityEngine;

namespace Wave.Patterns
{
    [CreateAssetMenu(menuName = "Waves/Patterns/Grid (Mask)")]
    public class GridSpawnPattern : SpawnPattern
    {
        public int width = 5;
        public int height = 5;
        public float spacing = 2f;

        [SerializeField]
        private bool[] mask;

        private void OnValidate()
        {
            EnsureMask();
        }

        public void EnsureMask()
        {
            var size = width * height;

            if (mask == null || mask.Length != size)
            {
                mask = new bool[size];
                for (int i = 0; i < size; i++)
                    mask[i] = true;
            }
        }
        
        public override void GeneratePositions(
            Vector3 origin,
            EnemyDataSo enemy,
            int count,
            List<Vector3> outPositions)
        {
            EnsureMask();
            outPositions.Clear();

            var activeCells = new List<int>();
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                    activeCells.Add(i);
            }

            if (activeCells.Count == 0)
                return;

            var half = spacing * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var cellIndex = activeCells[i % activeCells.Count];

                int x = cellIndex % width;
                int z = cellIndex / width;

                var cellCenter = origin +
                                 new Vector3(
                                     (x - (width - 1) * 0.5f) * spacing,
                                     0f,
                                     (z - (height - 1) * 0.5f) * spacing
                                 );

                var offset = new Vector3(
                    Random.Range(-half, half),
                    0f,
                    Random.Range(-half, half)
                );

                outPositions.Add(cellCenter + offset);
            }
        }

        public bool GetCell(int x, int y)
            => mask[y * width + x];

        public void SetCell(int x, int y, bool value)
            => mask[y * width + x] = value;
        
        public void SetAll(bool value)
        {
            EnsureMask();

            for (var i = 0; i < mask.Length; i++)
                mask[i] = value;
        }

        public void Invert()
        {
            EnsureMask();

            for (var i = 0; i < mask.Length; i++)
                mask[i] = !mask[i];
        }
    }
}