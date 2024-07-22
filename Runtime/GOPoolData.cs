using System;
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
        public IObjectPool<IGOPoolItem> Pool { get; set; }
        
        public GOPoolData(string key, GameObject prefab, IObjectPool<IGOPoolItem> pool)
        {
            _key = key;
            _prefab = prefab;
            Pool = pool;
        }

        public void Clear()
        {
            Pool.Clear();
            
            _key = null;
            _prefab = null;
            Pool = null;
        }
    }
}