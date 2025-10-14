using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BlockPoolManager : MonoBehaviour
{
    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private HashSet<GameObject> activeObjects = new(); // track active ones

    public GameObject Get(GameObject prefab, Transform parent = null)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(prefab);
                    var pooled = obj.AddComponent<BlockType>();
                    pooled.prefabRef = prefab;
                    obj.transform.position = new Vector2(-100f, 100f);
                    return obj;
                },
                actionOnGet: (obj) =>
                {
                    activeObjects.Add(obj);
                },
                actionOnRelease: (obj) =>
                {
                    obj.transform.position = new Vector2(-100f, 100f);
                    activeObjects.Remove(obj);
                },
                actionOnDestroy: (obj) =>
                {
                    Destroy(obj);
                },
                defaultCapacity: 100,   // optional
                maxSize: 500            // optional
            );

            pools.Add(prefab, pool);
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

        if (!activeObjects.Contains(instance))
            return;

        if (pools.TryGetValue(pooled.prefabRef, out var pool))
            pool.Release(instance);
        else
            Destroy(instance);
    }
}
