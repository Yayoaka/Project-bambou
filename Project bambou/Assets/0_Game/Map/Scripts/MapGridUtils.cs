using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Map
{
    public class MapGridUtils
    {
        private const float CellSize = 20f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetCellIndex(float3 position)
        {
            return (int2)math.floor(position.xz / CellSize);
        }
    }
}
