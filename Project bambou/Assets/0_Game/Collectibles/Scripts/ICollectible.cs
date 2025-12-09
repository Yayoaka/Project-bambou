namespace Collectibles
{
    public interface ICollectible
    {
        /// <summary>
        /// Indique si l'objet est déjà ramassé pour éviter un double trigger.
        /// </summary>
        bool IsCollected { get; }
        
        public void Init();
    }
}