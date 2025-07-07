using System;
using Plugins;
using UnityEngine;

public class MockMungtangi : MonoSingleton<MockMungtangi>
{
    protected override void Initialize()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
#endif
    }
}
