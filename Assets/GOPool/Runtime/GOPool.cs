using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace DarkNaku.GOPool {
    public class GOPool : MonoBehaviour {
        public static GOPool Instance {
            get {
                if (_isDestroyed) return null;

                lock (_lock) {
                    if (_instance == null) {
                        var instances = FindObjectsByType<GOPool>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                        if (instances.Length > 0) {
                            _instance = instances[0];

                            for (int i = 1; i < instances.Length; i++) {
                                Debug.LogWarningFormat("[GOPool] Instance Duplicated - {0}", instances[i].name);
                                Destroy(instances[i]);
                            }
                        } else {
                            _instance = new GameObject($"[Singleton] GOPool").AddComponent<GOPool>();
                        }
                    }

                    return _instance;
                }
            }
        }

        private static readonly object _lock = new();
        private static GOPool _instance;
        private static bool _isDestroyed;

        private Dictionary<string, GOPoolData> _moldTable = new();
        private bool _isReleaserRunning;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void PackageImportHandler() {
            var define = $"DARKNAKU_{MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Name.ToUpper()}";

            System.Array buildTargets = System.Enum.GetValues(typeof(BuildTarget));

            foreach (BuildTarget target in buildTargets) {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);

                if (BuildPipeline.IsBuildTargetSupported(buildTargetGroup, target) == false) continue;

#if UNITY_2023_1_OR_NEWER
                var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

                if (defines.IndexOf(define) > 0) continue;

#if UNITY_2023_1_OR_NEWER
				PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, $"{defines};{define}".Replace(";;", ";"));
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, $"{defines};{define}".Replace(";;", ";"));
#endif
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration() {
            _instance = null;
            _isDestroyed = false;
        }

        public static GameObject Get(string key, Transform parent = null) => Instance?._Get(key, parent)?.GO;
        public static T Get<T>(string key, Transform parent = null) where T : class => Instance?._Get<T>(key, parent);
        public static void Release(GameObject item, float delay = 0f) => Instance?._Release(item, delay);
        public static void Release(IGOPoolItem item, float delay = 0f) => Instance?._Release(item, delay);
        public static void Clear() => Instance?._Clear();
        public static async Task Preload(string key, int count) => await Instance._Preload(key, count);

        private void Awake() {
            if (_instance == null) {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (_instance != this) {
                Debug.LogWarning($"[GOPool] Duplicated - {name}");
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit() {
            if (_instance != this) return;

            _instance = null;
            _isDestroyed = true;

            _Clear();

            Debug.Log($"[GOPool] Destroyed.");
        }
        
        private void Register(string key, GameObject prefab, AsyncOperationHandle<GameObject> handle) {
            if (prefab == null) {
                Debug.LogError($"[GOPool] Register : Prefab is null.");
                return;
            }

            var pool = new ObjectPool<IGOPoolItem>(
                () => {
                    var go = Instantiate(prefab);

                    var item = go.GetComponent<IGOPoolItem>();

                    if (item == null) {
                        item = go.AddComponent<GOPoolItem>();
                    }

                    item.OnCreateItem();

                    return item;
                },
                OnGetItem,
                OnReleaseItem,
                OnDestroyItem);

            var data = new GOPoolData(pool, handle);

            _moldTable.TryAdd(key, data);
        }

        private bool Load(string key) {
            var go = Resources.Load<GameObject>(key);

            if (go == null) {
                try {
                    var handle = Addressables.LoadAssetAsync<GameObject>(key);
                    go = handle.WaitForCompletion();
                    Register(key, go, handle);
                    
                    return true;
                } catch (InvalidKeyException e) {
                    Debug.LogError($"[GOPool] Load : {e.Message}");
                    return false;   
                }
            }

            Register(key, go, default);
                
            return true;
        }
        
        private IGOPoolItem _Get(string key, Transform parent) {
            if (_moldTable.ContainsKey(key) == false) {
                if (Load(key) == false) return null;
            }
            
            if (_moldTable.TryGetValue(key, out var data)) {
                var item = data.Get();

                item.GO.transform.SetParent(parent);
                
                return item;
            }

            return null;
        }
        
        private T _Get<T>(string key, Transform parent) where T : class {
            return _Get(key, parent)?.GO.GetComponent<T>();
        }

        private async Task ReleaseAsync(IGOPoolItem item, float delay) {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            
            item.PoolData.Release(item);
        }

        private void _Release(IGOPoolItem item, float delay) {
            if (item == null) return;

            _ = ReleaseAsync(item, delay);
        }

        private void _Release(GameObject item, float delay) {
            if (item == null) return;
            
            _Release(item.GetComponent<IGOPoolItem>(), delay);
        }

        private void _Clear() {
            foreach (var data in _moldTable.Values) {
                data.Clear();
            }

            _moldTable.Clear();
            
            Resources.UnloadUnusedAssets();
        }
        
        private async Task _Preload(string key, int count) {
            var frameTime = 1f / Application.targetFrameRate;
            var items = ListPool<IGOPoolItem>.Get();
            var startTime = Time.realtimeSinceStartup;

            while (count > 0) {
                var item = _Get(key, transform);

                item.GO.SetActive(false);

                items.Add(item);

                count--;

                if (Time.realtimeSinceStartup - startTime >= frameTime) {
                    await Task.Yield();
                    startTime = Time.realtimeSinceStartup;
                }
            }

            for (int i = 0; i < items.Count; i++) {
                _Release(items[i], 0f);
            }
            
            ListPool<IGOPoolItem>.Release(items);
        }

        private void OnGetItem(IGOPoolItem item) {
            item.OnGetItem();
        }

        private void OnReleaseItem(IGOPoolItem item) {
            item.OnReleaseItem();
        }

        private void OnDestroyItem(IGOPoolItem item) {
            item.OnDestroyItem();

            Destroy(item.GO);
        }
    }
}