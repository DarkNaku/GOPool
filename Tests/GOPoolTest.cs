using System.Collections;
using System.Collections.Generic;
using DarkNaku.GOPool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GOPoolTest
{
    [UnityTest]
    public IEnumerator GOPoolTestWithEnumeratorPasses()
    {
        GOPool.RegisterBuiltIn("Prefabs/Capsule", "Prefabs/Cube", "Prefabs/Sphere");
        
        yield return CoCreate("Capsule");
        yield return CoCreate("Cube");
        yield return CoCreate("Sphere");
        
        yield return CoCreate("Capsule");
        yield return CoCreate("Cube");
        yield return CoCreate("Sphere");
        
        var capsule = GOPool.Get("Capsule");
        var cube = GOPool.Get("Cube");
        var sphere = GOPool.Get("Sphere");
        
        GOPool.Release(capsule, 1.5f);
        GOPool.Release(cube, 1f);
        GOPool.Release(sphere, 0.5f);
        
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator CoCreate(string key)
    {
        var goes = new List<GameObject>();

        for (int i = 0; i < 10; i++)
        {
            var go = GOPool.Get(key);
            
            go.transform.position = Random.insideUnitSphere * 5f;
            
            goes.Add(go);

            yield return null;
        }

        foreach (var go in goes)
        {
            GOPool.Release(go);
        }
    }
}
