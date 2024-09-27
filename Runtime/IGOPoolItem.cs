using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool
{
    public interface IGOPoolItem
    {
        GameObject GO { get; }
        GOPoolData Data{ get; set; }

        void OnGetItem();
        void OnReleaseItem();
        void OnDestroyItem();
    }
}