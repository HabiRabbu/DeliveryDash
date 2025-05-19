using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AvailableJobsUI : MonoBehaviour
{
    [SerializeField] private GameObject offerPrefab;
    [SerializeField] private Transform container;

    private PlayerControls controls;
    private UIPrefabPool pool;
    private readonly List<AvailableJobEntry> entries = new();

    void Awake()
    {
        pool = new UIPrefabPool(offerPrefab, container);
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        DeliveryEvents.OnOfferCreated += addEntry;
        DeliveryEvents.OnOfferExpired += removeEntry;
        DeliveryEvents.OnJobAccepted += removeEntry;

        controls.UI.Enable();
        controls.UI.AcceptJob.performed += _ => AcceptJob();
        controls.UI.DeclineJob.performed += _ => DeclineJob();
    }

    void OnDisable()
    {
        DeliveryEvents.OnOfferCreated -= addEntry;
        DeliveryEvents.OnOfferExpired -= removeEntry;
        DeliveryEvents.OnJobAccepted -= removeEntry;

        controls.UI.AcceptJob.performed -= _ => AcceptJob();
        controls.UI.DeclineJob.performed -= _ => DeclineJob();
        controls.UI.Disable();
    }

    //TODO: Logic so that the buttons only show when it's the last in the list. Calls .Update() after every add/remove
    private void addEntry(DeliveryJob job)
    {
        var go = pool.Get();
        var entry = go.GetComponent<AvailableJobEntry>();
        entry.Setup(
            job,
            (j, g) => DeliveryEvents.JobAccepted(j),
            (j, g) => DeliveryEvents.OfferExpired(j)
        );
        entries.Add(entry);
    }

    private void removeEntry(DeliveryJob job)
    {
        var entry = entries.Find(e => e.jobId == job.id);
        if (entry != null)
        {
            entries.Remove(entry);
            pool.Release(entry.gameObject);
        }
    }

    private void AcceptJob()
    {
        if (entries.Count > 0)
        {
            entries[entries.Count - 1].Accept();
        }
    }

    private void DeclineJob()
    {
        if (entries.Count > 0)
        {
            entries[entries.Count - 1].Decline();
        }
    }
}
