using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool {
    public class GOPoolItem : MonoBehaviour, IGOPoolItem {
        public GameObject GO => gameObject;
        public GOPoolData PoolData { get; set; }

        public void OnCreateItem() {
            OnCreateItemInternal();
        }

        public void OnGetItem() {
            GO.SetActive(true);

            OnGetItemInternal();
        }

        public void OnReleaseItem() {
            GO.SetActive(false);

            OnReleaseItemInternal();
        }

        public void OnDestroyItem() {
            OnDestroyItemInternal();
        }

        protected virtual void OnCreateItemInternal() {
        }

        protected virtual void OnGetItemInternal() {
        }

        protected virtual void OnReleaseItemInternal() {
        }

        protected virtual void OnDestroyItemInternal() {
        }
    }
}