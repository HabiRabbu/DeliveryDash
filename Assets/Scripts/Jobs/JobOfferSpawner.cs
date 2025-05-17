using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobOfferSpawner : MonoBehaviour
{
    [Header("Zone Roots (assign your PickupLocations and DeliveryLocations containers)")]
    [SerializeField] private Transform pickupLocationsRoot;
    [SerializeField] private Transform deliveryLocationsRoot;

    private DeliveryZone[] pickupZones;
    private DeliveryZone[] deliveryZones;

    [Header("Spawn Settings")]
    [SerializeField] private float minSpawn = 10f;
    [SerializeField] private float maxSpawn = 30f;
    [SerializeField] private float acceptDuration = 20f;
    [SerializeField] private float minDeliveryTime = 60f;
    [SerializeField] private float maxDeliveryTime = 200f;

    // Tracks jobs offered but not yet accepted or expired
    private readonly List<DeliveryJob> availableOffers = new List<DeliveryJob>();

    private Coroutine spawnLoop;

    void Awake()
    {
        // Find all DeliveryZone children under each root
        pickupZones = pickupLocationsRoot
            .GetComponentsInChildren<DeliveryZone>()
            .Where(z => z.zoneType == ZoneType.Pickup)
            .ToArray();

        deliveryZones = deliveryLocationsRoot
            .GetComponentsInChildren<DeliveryZone>()
            .Where(z => z.zoneType == ZoneType.Deliver)
            .ToArray();

        if (pickupZones.Length == 0 || deliveryZones.Length == 0)
            Debug.LogWarning("JobOfferSpawner: zone roots missing DeliveryZone children!");
    }

    void OnEnable()
    {
        // Start the spawn loop
        spawnLoop = StartCoroutine(SpawnLoop());

        DeliveryEvents.OnOfferExpired += HandleOfferRemoved;
        DeliveryEvents.OnJobAccepted   += HandleOfferRemoved;
    }

    void OnDisable()
    {
        if (spawnLoop != null)
            StopCoroutine(spawnLoop);

        DeliveryEvents.OnOfferExpired -= HandleOfferRemoved;
        DeliveryEvents.OnJobAccepted   -= HandleOfferRemoved;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawn, maxSpawn));
            SpawnOffer();
        }
    }

    void SpawnOffer()
    {
        // Find every possible pair (TODO: Potential lag creator.....)
        var allPairs = new List<(DeliveryZone pu, DeliveryZone doff)>();
        foreach (var pu in pickupZones)
            foreach (var doff in deliveryZones)
                if (pu != doff)
                    allPairs.Add((pu, doff));

        // Filter out already offered or already active
        var valid = allPairs.Where(pair =>
            !availableOffers.Any(j =>
                j.pickupZone == pair.pu && j.dropoffZone == pair.doff)
            && !JobManager.Instance.GetActiveJobs().Any(j =>
                j.pickupZone == pair.pu && j.dropoffZone == pair.doff)
        ).ToList();

        if (valid.Count == 0)
            return;

        // Pick one at random
        var (pickup, dropoff) = valid[UnityEngine.Random.Range(0, valid.Count)];
        var job = new DeliveryJob(
            Guid.NewGuid().ToString(),
            pickup,
            dropoff,
            UnityEngine.Random.Range(minDeliveryTime, maxDeliveryTime)
        );

        //Record and broadcast
        availableOffers.Add(job);
        DeliveryEvents.OfferCreated(job);

        // Schedule automatic expiration if the player never accepts
        TimerService.Instance.Schedule(acceptDuration, () =>
        {
            if (availableOffers.Remove(job))
                DeliveryEvents.OfferExpired(job);
        });
    }

    /// <summary>
    /// Removes a job from availableOffers when it either expires or is accepted.
    /// </summary>
    private void HandleOfferRemoved(DeliveryJob job)
    {
        availableOffers.Remove(job);
    }
}
