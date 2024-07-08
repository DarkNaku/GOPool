using System;
using UnityEngine;
using UnityEngine.Pool;

namespace DarkNaku.GOPool
{
    [Serializable]
    public struct GOPoolData
    {
        public string Key { get; private set; }
        public GameObject Prefab { get; private set; }
        public IObjectPool<IGOPoolItem> Pool { get; private set; }
        
        public GOPoolData(string key, GameObject prefab, IObjectPool<IGOPoolItem> pool)
        {
            Key = key;
            Prefab = prefab;
            Pool = pool;
        }

        public void Clear()
        {
            Pool.Clear();
            
            Key = null;
            Prefab = null;
            Pool = null;
        }
    }
}