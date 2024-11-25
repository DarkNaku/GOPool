using System.Collections;
using System.Collections.Generic;
using DarkNaku.GOPool;
using UnityEngine;

public class Sample : MonoBehaviour
{
    private void Awake() {
        _ = GOPool.Preload("Cube", 1000);
    }
}
