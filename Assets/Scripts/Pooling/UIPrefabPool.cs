using UnityEngine;

/// <summary>
/// Wraps PoolManager for UI so you only need prefab + parent once. (Z-index issues)
/// </summary>
public class UIPrefabPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;

    public UIPrefabPool(GameObject prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public GameObject Get()
    {
        return PoolManager.Instance.Get(prefab, parent);
    }

    public void Release(GameObject instance)
    {
        PoolManager.Instance.Release(prefab, instance);
    }
}
