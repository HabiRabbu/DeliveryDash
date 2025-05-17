using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and reclaims ActiveJobEntry panels in response to job events.
/// </summary>
public class ActiveJobsUI : MonoBehaviour
{
    [SerializeField] private GameObject activePrefab;
    [SerializeField] private Transform  container;

    private UIPrefabPool pool;
    private readonly Dictionary<string, GameObject> entries = new();

    void Awake()
    {
        pool = new UIPrefabPool(activePrefab, container);
    }

    void OnEnable()
    {
        DeliveryEvents.OnJobAccepted  += AddEntry;
        DeliveryEvents.OnJobPickedUp  += UpdateEntry;
        DeliveryEvents.OnJobDelivered += RemoveEntry;
        DeliveryEvents.OnJobExpired   += RemoveEntry;
    }

    void OnDisable()
    {
        DeliveryEvents.OnJobAccepted  -= AddEntry;
        DeliveryEvents.OnJobPickedUp  -= UpdateEntry;
        DeliveryEvents.OnJobDelivered -= RemoveEntry;
        DeliveryEvents.OnJobExpired   -= RemoveEntry;
    }

    private void AddEntry(DeliveryJob job)
    {
        if (entries.ContainsKey(job.id)) return;
        var go = pool.Get();
        entries[job.id] = go;
        go.GetComponent<ActiveJobEntry>().Setup(job);
    }

    private void UpdateEntry(DeliveryJob job)
    {
        // TODO: change color/icon/text to show “picked up” state
        if (entries.TryGetValue(job.id, out var go))
            go.GetComponent<ActiveJobEntry>().Setup(job);
    }

    private void RemoveEntry(DeliveryJob job)
    {
        if (!entries.TryGetValue(job.id, out var go)) return;
        pool.Release(go);
        entries.Remove(job.id);
    }
}
