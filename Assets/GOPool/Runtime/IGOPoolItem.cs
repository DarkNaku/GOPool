using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool
{
    public interface IGOPoolItem
    {
        GameObject GO { get; }
        GOPoolData PoolData { get; set; }

        void OnGetItem();
        void OnReleaseItem();
        void OnDestroyItem();
    }
}