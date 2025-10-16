using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BlockPoolManager : MonoBehaviour
{
    private Dictionary<string, ObjectPool<GameObject>> pools = new();

    public GameObject Get(GameObject prefab, Transform parent = null)
    {
        if (!pools.TryGetValue(prefab.name, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab);
                    var pooled = obj.GetComponent<BlockType>();
                    pooled.prefabName = prefab.name;
                    obj.SetActive(false);
                    return obj;
                },
                actionOnGet: (obj) =>
                {
                    obj.SetActive(true);
                },
                actionOnRelease: (obj) =>
                {
                    obj.SetActive(false);
                },
                actionOnDestroy: (obj) =>
                {
                    Destroy(obj);
                }
            );

            pools.Add(prefab.name, pool);
        }

        GameObject instance = pool.Get();
        if (parent != null)
            instance.transform.SetParent(parent, false);

        return instance;
    }

    public void Release(GameObject instance)
    {
        if (!instance) return;

        var pooled = instance.GetComponent<BlockType>();
        if (pooled == null) { Destroy(instance); return; }

        if (pools.TryGetValue(pooled.prefabName, out var pool))
            pool.Release(instance);
        else
            Destroy(instance);
    }
}
