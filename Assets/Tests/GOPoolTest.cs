using System.Collections;
using System.Collections.Generic;
using DarkNaku.GOPool;
using UnityEngine;
using UnityEngine.TestTools;

public class GOPoolTest
{
    [UnityTest]
    public IEnumerator GOPoolTestWithEnumeratorPasses() {
        var awaiter = GOPool.Preload("Capsule", 10).GetAwaiter();

        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        awaiter = GOPool.Preload("Prefabs/Cube", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        awaiter = GOPool.Preload("Sphere", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        yield return new WaitForSeconds(3f);

        CreateAndRelease("Capsule", 10, 1f);
        CreateAndRelease("Prefabs/Cube", 10, 1f);
        CreateAndRelease("Sphere", 10, 1f);
        
        yield return new WaitForSeconds(3f);

        CreateAndRelease("Capsule", 10, 1f);
        CreateAndRelease("Prefabs/Cube", 10, 1f);
        CreateAndRelease("Sphere", 10, 1f);
        
        yield return new WaitForSeconds(3f);

        var capsule = GOPool.Get("Capsule");
        var cube = GOPool.Get("Prefabs/Cube");
        var sphere = GOPool.Get("Sphere");
        
        GOPool.Release(capsule, 1.5f);
        GOPool.Release(cube, 1f);
        GOPool.Release(sphere, 0.5f);
        
        yield return new WaitForSeconds(3f);
    }
    
    private void CreateAndRelease(string key, int count, float releaseDelay) {
        var goes = new List<GameObject>();

        for (int i = 0; i < count; i++) {
            var go = GOPool.Get(key);
            
            go.transform.position = Random.insideUnitSphere * 5f;
            
            goes.Add(go);
        }

        foreach (var go in goes) {
            GOPool.Release(go, releaseDelay);
        }
    }
}
