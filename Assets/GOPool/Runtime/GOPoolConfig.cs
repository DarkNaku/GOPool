using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkNaku.GOPool
{
    public class GOPoolConfig : ScriptableObject
    {
        [SerializeField] private List<GOPoolData> _items = new List<GOPoolData>();
            
        public static GOPoolConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    var assetName = typeof(GOPoolConfig).Name;
                
                    _instance = Resources.Load<GOPoolConfig>(assetName);

                    if (_instance == null)
                    {
                        _instance = CreateInstance<GOPoolConfig>();
                    
#if UNITY_EDITOR
                        var assetPath = "Resources";
                        var resourcePath = System.IO.Path.Combine(Application.dataPath, assetPath);

                        if (System.IO.Directory.Exists(resourcePath) == false)
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", assetPath);
                        }

                        UnityEditor.AssetDatabase.CreateAsset(_instance, $"Assets/{assetPath}/{assetName}.asset");
#endif
                    }
                }
            
                return _instance;
            }
        }

        public static IReadOnlyList<GOPoolData> Items => Instance._items;
    
        private static GOPoolConfig _instance;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/GOPool Config")]
        private static void SelectConfig()
        {
            UnityEditor.Selection.activeObject = Instance;
        }
#endif
    }
}