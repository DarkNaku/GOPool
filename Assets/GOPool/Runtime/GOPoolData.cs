using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DarkNaku.GOPool {
    [Serializable]
    public class GOPoolData {
        private IObjectPool<IGOPoolItem> _pool;
        private AsyncOperationHandle<GameObject> _handle;
        private HashSet<IGOPoolItem> _inactiveItems = new();

        public GOPoolData(IObjectPool<IGOPoolItem> pool, AsyncOperationHandle<GameObject> handle) {
            _pool = pool;
            _handle = handle;
        }

        public IGOPoolItem Get() {
            var item = _pool.Get();

            item.PoolData = this;

            _inactiveItems.Add(item);

            return item;
        }

        public void Release(IGOPoolItem item) {
            _pool.Release(item);

            _inactiveItems.Remove(item);
        }

        public void Clear() {
            foreach (var item in _inactiveItems) {
                _pool.Release(item);
            }

            if (_handle.IsValid()) {
                Addressables.Release(_handle);
            }

            _inactiveItems.Clear();
            _pool.Clear();
            
            _pool = null;
        }
    }
}