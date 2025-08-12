using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DarkNaku.GOPool;
using UnityEngine;
using UnityEngine.TestTools;

public class GOPoolTest
{
    [UnityTest]
    public IEnumerator GOPoolTestWithEnumeratorPasses() {
        var awaiter = GOPool.Preload("Capsule", 10).GetAwaiter();

        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        awaiter = GOPool.PreloadBuiltIn("Prefabs/Cube", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        awaiter = GOPool.Preload("Sphere", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        yield return new WaitForSeconds(3f);
        
        awaiter = CreateAsync("Capsule", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        yield return CoCreateBuiltIn("Prefabs/Cube");
        
        awaiter = CreateAsync("Sphere", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        yield return new WaitForSeconds(3f);
        
        awaiter = CreateAsync("Capsule", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);
        
        yield return CoCreateBuiltIn("Prefabs/Cube");
        
        awaiter = CreateAsync("Sphere", 10).GetAwaiter();
        
        yield return new WaitUntil(() => awaiter.IsCompleted);

        var awaiter2 = GOPool.Get("Capsule").GetAwaiter();

        yield return new WaitUntil(() => awaiter2.IsCompleted);
        
        var capsule = awaiter2.GetResult();
        
        var cube = GOPool.GetBuiltIn("Prefabs/Cube");
        
        awaiter2 = GOPool.Get("Sphere").GetAwaiter();
        
        yield return new WaitUntil(() => awaiter2.IsCompleted);
        
        var sphere = awaiter2.GetResult();
        
        GOPool.Release(capsule, 1.5f);
        GOPool.Release(cube, 1f);
        GOPool.Release(sphere, 0.5f);
        
        yield return new WaitForSeconds(3f);
    }
    
    private async UniTask CreateAsync(string key, int count) {
        var goes = new List<GameObject>();

        for (int i = 0; i < count; i++) {
            var go = await GOPool.Get(key);
            
            go.transform.position = Random.insideUnitSphere * 5f;
            
            goes.Add(go);
        }

        foreach (var go in goes) {
            GOPool.Release(go);
        }
    }

    private IEnumerator CoCreateBuiltIn(string key)
    {
        var goes = new List<GameObject>();

        for (int i = 0; i < 10; i++) {
            var go = GOPool.GetBuiltIn(key);
            
            go.transform.position = Random.insideUnitSphere * 5f;
            
            goes.Add(go);

            yield return null;
        }

        foreach (var go in goes) {
            GOPool.Release(go);
        }
    }
}
