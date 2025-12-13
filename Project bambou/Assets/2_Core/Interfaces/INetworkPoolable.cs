namespace Interfaces
{
    public interface INetworkPoolable
    {
        void OnPoolAcquire();
        void OnPoolRelease();
    }
}