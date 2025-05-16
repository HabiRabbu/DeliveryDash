using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance { get; private set; }

    [Header("Zone Roots")]
    [SerializeField] private Transform pickupLocationsRoot;
    [SerializeField] private Transform deliveryLocationsRoot;

    [Header("Job Timing")]
    [SerializeField] private float minDeliveryTime = 60f;
    [SerializeField] private float maxDeliveryTime = 200f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    // Caches for zones and jobs
    private List<DeliveryZone> pickupZones;
    private List<DeliveryZone> deliveryZones;
    private List<DeliveryJob> activeJobs = new List<DeliveryJob>();

    [Header("Offers & UI")]
    private List<DeliveryJob> availableJobs = new List<DeliveryJob>();
    [SerializeField] private float minSpawnInterval = 10f, maxSpawnInterval = 30f;
    [SerializeField] private float acceptDuration = 20f;
    [SerializeField] private Transform availableContainer;
    [SerializeField] private GameObject availableEntryPrefab;
    [SerializeField] private Transform activeContainer;
    [SerializeField] private GameObject activeEntryPrefab;

    private Coroutine offerSpawner;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeZones();
        // TODO: validate that pickupLocationsRoot and deliveryLocationsRoot are assigned to avoid null refs
    }

    private void Start()
    {
        // Begin spawning occasional job offers
        offerSpawner = StartCoroutine(SpawnOffers());

        // Ensure zones reflect current jobs (initial sync)
        UpdateZoneIndicators();
    }

    private void OnDisable()
    {
        if (offerSpawner != null)
            StopCoroutine(offerSpawner);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        // Iterate backwards to safely remove expired jobs
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];
            job.UpdateTimer(dt);
            if (job.State == JobState.Expired)
            {
                if (debug) Debug.Log($"Job expired: {job.id}");
                activeJobs.RemoveAt(i);
                UpdateZoneIndicators();
                // TODO: Notify UI of expiration via event rather than polling
            }
        }
    }

    private void InitializeZones()
    {
        pickupZones = new List<DeliveryZone>();
        deliveryZones = new List<DeliveryZone>();

        foreach (Transform child in pickupLocationsRoot)
        {
            var zone = child.GetComponentInChildren<DeliveryZone>();
            if (zone != null && zone.zoneType == ZoneType.Pickup)
                pickupZones.Add(zone);
        }
        foreach (Transform child in deliveryLocationsRoot)
        {
            var zone = child.GetComponentInChildren<DeliveryZone>();
            if (zone != null && zone.zoneType == ZoneType.Deliver)
                deliveryZones.Add(zone);
        }
        if (debug)
            Debug.Log($"Found {pickupZones.Count} pickup zones and {deliveryZones.Count} delivery zones.");
    }

    /// <summary>
    /// Handle player entering any DeliveryZone.
    /// </summary>
    public void HandleZoneTrigger(DeliveryZone zone)
    {
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];

            // Pickup phase
            if (job.State == JobState.PendingPickup && job.pickupZone == zone)
            {
                job.Pickup();
                if (debug) Debug.Log($"Picked up job {job.id}");
                UpdateZoneIndicators();
                // TODO: decouple UI updates from game logic (use events)
            }
            // Dropoff phase
            else if (job.State == JobState.PendingDropoff && job.dropoffZone == zone)
            {
                job.Deliver();
                if (debug) Debug.Log($"Delivered job {job.id}");
                activeJobs.RemoveAt(i);
                UpdateZoneIndicators();
                // TODO: consider pooling job entries instead of destroy/instantiate for performance
            }
        }
    }

    /// <summary>
    /// Refresh visual indicators on all zones based on job states.
    /// </summary>
    public void UpdateZoneIndicators()
    {
        // Show pickups
        foreach (var zone in pickupZones)
        {
            bool show = activeJobs.Exists(j => j.State == JobState.PendingPickup && j.pickupZone == zone);
            zone.SetIndicator(show);
        }
        // Show dropoffs
        foreach (var zone in deliveryZones)
        {
            bool show = activeJobs.Exists(j => j.State == JobState.PendingDropoff && j.dropoffZone == zone);
            zone.SetIndicator(show);
        }
    }

    // ---- Job Offers Logic ----
    private IEnumerator SpawnOffers()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval));
            SpawnOffer();
        }
    }

    private void SpawnOffer()
    {
        // Build list of valid pairs; consider caching or limiting frequency to reduce allocations
        var validPairs = new List<KeyValuePair<DeliveryZone, DeliveryZone>>();
        foreach (var pu in pickupZones)
        {
            foreach (var doff in deliveryZones)
            {
                if (pu == doff) continue;
                if (availableJobs.Any(j => j.pickupZone == pu && j.dropoffZone == doff) ||
                    activeJobs.Any(j => j.pickupZone == pu && j.dropoffZone == doff))
                    continue;
                validPairs.Add(new KeyValuePair<DeliveryZone, DeliveryZone>(pu, doff));
            }
        }
        if (validPairs.Count == 0)
        {
            if (debug) Debug.Log("No more unique jobs available.");
            return;
        }

        var choice = validPairs[UnityEngine.Random.Range(0, validPairs.Count)];
        var job = new DeliveryJob(Guid.NewGuid().ToString(), choice.Key, choice.Value, UnityEngine.Random.Range(minDeliveryTime, maxDeliveryTime));
        availableJobs.Add(job);

        // Instantiate UI entry; consider object pooling for frequent spawns/despawn
        var entryGO = Instantiate(availableEntryPrefab);
        entryGO.transform.SetParent(availableContainer, false);
        entryGO.GetComponent<AvailableJobEntry>().Setup(job, OnAccept, OnDeny);

        // Auto-expire
        StartCoroutine(AutoExpire(job, entryGO));
    }

    private IEnumerator AutoExpire(DeliveryJob job, GameObject entryGO)
    {
        yield return new WaitForSeconds(acceptDuration);
        if (availableJobs.Remove(job))
        {
            Destroy(entryGO);
            if (debug) Debug.Log($"Offer expired: {job.id}");
        }
    }

    private void OnAccept(DeliveryJob job, GameObject entryGO)
    {
        availableJobs.Remove(job);
        Destroy(entryGO);

        activeJobs.Add(job);
        if (debug) Debug.Log($"Accepted job {job.id}");
        UpdateZoneIndicators();

        // Instantiate active job UI entry
        var go = Instantiate(activeEntryPrefab);
        go.transform.SetParent(activeContainer, false);
        go.GetComponent<ActiveJobEntry>().Setup(job);
    }

    private void OnDeny(DeliveryJob job, GameObject entryGO)
    {
        availableJobs.Remove(job);
        Destroy(entryGO);
        if (debug) Debug.Log($"Denied job {job.id}");
    }

    public IReadOnlyList<DeliveryJob> GetActiveJobs() => activeJobs;
}