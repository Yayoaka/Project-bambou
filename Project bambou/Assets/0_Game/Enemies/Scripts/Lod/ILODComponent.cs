namespace Enemies.Lod
{
    public interface ILODComponent
    {
        public enum LodLevel
        {
            High,
            Medium,
            Low
        }
        
        void SetLOD(LodLevel level);
    }
}
