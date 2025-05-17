using System.Collections.Generic;
using UnityEngine;

public class AvailableJobsUI : MonoBehaviour
{
    [SerializeField] private GameObject offerPrefab;
    [SerializeField] private Transform  container;

    private UIPrefabPool pool;
    private readonly Dictionary<string, GameObject> activeOfferEntries = new();

    void Awake()
    {
        pool = new UIPrefabPool(offerPrefab, container);
    }

    void OnEnable()
    {
        DeliveryEvents.OnOfferCreated += ShowOffer;
        DeliveryEvents.OnOfferExpired += HideOffer;
        DeliveryEvents.OnJobAccepted  += HideOffer;
    }

    void OnDisable()
    {
        DeliveryEvents.OnOfferCreated -= ShowOffer;
        DeliveryEvents.OnOfferExpired -= HideOffer;
        DeliveryEvents.OnJobAccepted  -= HideOffer;
    }

    private void ShowOffer(DeliveryJob job)
    {
        var go = pool.Get();
        activeOfferEntries[job.id] = go;

        var entry = go.GetComponent<AvailableJobEntry>();
        entry.Setup(
            job,
            (j, g) => DeliveryEvents.JobAccepted(j),
            (j, g) => DeliveryEvents.OfferExpired(j)
        );
    }

    private void HideOffer(DeliveryJob job)
    {
        if (activeOfferEntries.TryGetValue(job.id, out var go))
        {
            pool.Release(go);
            activeOfferEntries.Remove(job.id);
        }
    }
}
