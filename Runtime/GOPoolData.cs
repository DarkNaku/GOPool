using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool
{
    [Serializable]
    public class GOPoolData
    {
        [SerializeField] private string _key;
        [SerializeField] private GameObject _prefab;

        public string Key => _key;
        public GameObject Prefab => _prefab;
        
        private IObjectPool<IGOPoolItem> _pool;
        private HashSet<IGOPoolItem> _inactiveItems = new();
        
        public GOPoolData(string key, GameObject prefab, IObjectPool<IGOPoolItem> pool)
        {
            _key = key;
            _prefab = prefab;
            _pool = pool;
        }

        public IGOPoolItem Get()
        {
            var item = _pool.Get();

            item.PoolData = this;

            _inactiveItems.Add(item);

            return item;
        }

        public void Release(IGOPoolItem item)
        {
            _pool.Release(item);
            
            _inactiveItems.Remove(item);
        }

        public void Clear()
        {
            foreach (var item in _inactiveItems)
            {
                _pool.Release(item);
            }
            
            _inactiveItems.Clear();
            _pool.Clear();
            
            _key = null;
            _prefab = null;
            _pool = null;
        }
    }
}