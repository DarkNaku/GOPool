using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool
{
    public class GOPoolItem : MonoBehaviour, IGOPoolItem
    {
        public GameObject GO => gameObject;
        public GOPoolData PoolData { get; set; }

        public virtual void OnGetItem()
        {
            GO.SetActive(true);
        }

        public virtual void OnReleaseItem()
        {
            GO.SetActive(false);
        }

        public virtual void OnDestroyItem()
        {
        }
    }
}