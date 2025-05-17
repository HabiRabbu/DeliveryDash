using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic GameObject pooling.  
/// Call Get(prefab,parent) to reuse or Instantiate.  
/// Call Release(prefab, instance) to deactivate and enqueue.
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    public GameObject Get(GameObject prefab, Transform parent = null)
    {
        if (!pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            pools[prefab] = q;
        }

        GameObject go = (q.Count > 0) 
            ? q.Dequeue() 
            : Instantiate(prefab);
            
        go.transform.SetParent(parent, false);
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject prefab, GameObject instance)
    {
        instance.SetActive(false);
        pools[prefab].Enqueue(instance);
    }
}
