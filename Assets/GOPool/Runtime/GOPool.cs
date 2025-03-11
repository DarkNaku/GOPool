using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

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
        private static bool _configRegistered;

        private Dictionary<string, GOPoolData> _moldTable = new Dictionary<string, GOPoolData>();
        private Dictionary<IGOPoolItem, float> _releaseQueue = new Dictionary<IGOPoolItem, float>();
        private List<IGOPoolItem> _releaseItems = new List<IGOPoolItem>();
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
            _configRegistered = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad() {
            if (_configRegistered) return;

            for (int i = 0; i < GOPoolConfig.Items.Count; i++) {
                Instance._Register(GOPoolConfig.Items[i]);
            }

            _configRegistered = true;
        }

        public static void RegisterBuiltIn(params string[] paths) {
            Instance?._RegisterBuiltIn(paths);
        }

        public static void Register(string key, GameObject prefab) {
            Instance?._Register(key, prefab);
        }

        public static void Unregister(string key) {
            Instance?._Unregister(key);
        }

        public static GameObject Get(string key, Transform parent = null) {
            return Instance?._Get(key, parent).GO;
        }

        public static T Get<T>(string key, Transform parent = null) where T : class {
            return Instance?._Get<T>(key, parent);
        }

        public static void Release(GameObject item, float delay = 0f) {
            Instance?._Release(item, delay);
        }

        public static void Release(IGOPoolItem item, float delay = 0f) {
            Instance?._Release(item, delay);
        }

        public static void Clear() {
            Instance?._Clear();
        }

        public static async Task Preload(string key, int count) {
            await Instance._Preload(key, count);
        }

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

        private void _RegisterBuiltIn(params string[] paths) {
            if (paths == null) return;

            for (int i = 0; i < paths.Length; i++) {
                var prefab = Resources.Load<GameObject>(paths[i]);

                if (prefab == null) {
                    Debug.LogError($"[GOPool] RegisterBuiltIn : Prefab is null. Path = {paths[i]}");
                    continue;
                }

                var key = Path.GetFileNameWithoutExtension(paths[i]);

                if (string.IsNullOrEmpty(key)) {
                    _Register(paths[i], prefab);
                } else {
                    _Register(key, prefab);
                }
            }
        }

        private void _Register(GOPoolData data) {
            if (data == null) {
                Debug.LogError($"[GOPool] Register : Data is null.");
                return;
            }

            _Register(string.IsNullOrEmpty(data.Key) ? data.Prefab.name : data.Key, data.Prefab);
        }

        private void _Register(string key, GameObject prefab) {
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

            var data = new GOPoolData(key, prefab, pool);

            _moldTable.TryAdd(key, data);
        }

        private void _Unregister(string key) {
            if (_moldTable.ContainsKey(key)) {
                _moldTable[key].Clear();
            }

            _moldTable.Remove(key);

            Resources.UnloadUnusedAssets();
        }

        private T _Get<T>(string key, Transform parent) where T : class {
            var item = _Get(key, parent);

            return (item != null) ? item.GO.GetComponent<T>() : default;
        }

        private IGOPoolItem _Get(string key, Transform parent) {
            if (_moldTable.ContainsKey(key) == false) {
                var go = Resources.Load<GameObject>(key);

                if (!go) {
                    Debug.LogError($"[GOPool Get : Can't found item. {key}");
                    return null;
                }

                _Register(key, go);
            }

            if (_moldTable.TryGetValue(key, out var data)) {
                var item = data.Get();

                item.GO.transform.SetParent(parent);

                return item;
            }

            return default;
        }

        private void _Release(GameObject item, float delay) {
            _Release(item.GetComponent<IGOPoolItem>(), delay);
        }

        private void _Release(IGOPoolItem item, float delay) {
            if (item == null) return;

            if (delay > 0f) {
                StartCoroutine(CoRelease(item, delay));
            } else {
                item.PoolData.Release(item);
            }
        }

        private void _Clear() {
            _releaseItems.Clear();
            _releaseQueue.Clear();

            var keys = new List<string>(_moldTable.Keys);

            for (int i = 0; i < keys.Count; i++) {
                _moldTable[keys[i]].Clear();
            }

            _moldTable.Clear();
            Resources.UnloadUnusedAssets();
        }

        private async Task _Preload(string key, int count) {
            var frameTime = 1f / Application.targetFrameRate;

            if (_moldTable.ContainsKey(key)) {
                var items = new List<IGOPoolItem>();

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
            } else {
                Debug.LogWarning($"[GOPool] CoWarmUp : Can't found key - {key}");
            }
        }

        private IEnumerator CoRelease(IGOPoolItem item, float delay) {
            _releaseQueue[item] = delay;

            if (_isReleaserRunning) yield break;

            _isReleaserRunning = true;

            while (_releaseQueue.Count > 0) {
                yield return null;

                var keys = new List<IGOPoolItem>(_releaseQueue.Keys);

                for (int i = 0; i < keys.Count; i++) {
                    var key = keys[i];

                    if (_releaseQueue[key] > 0f) {
                        _releaseQueue[key] -= Time.deltaTime;
                    } else {
                        _releaseItems.Add(key);
                    }
                }

                for (int i = 0; i < _releaseItems.Count; i++) {
                    _Release(_releaseItems[i], 0f);
                    _releaseQueue.Remove(_releaseItems[i]);
                }

                _releaseItems.Clear();
            }

            _isReleaserRunning = false;
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
