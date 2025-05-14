using System.Collections;
using System.Collections.Generic;
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

    private List<DeliveryZone> pickupZones;
    private List<DeliveryZone> deliveryZones;
    private List<DeliveryJob> activeJobs = new List<DeliveryJob>();


    // Accepting Jobs
    private List<DeliveryJob> availableJobs = new List<DeliveryJob>();
    [Header("Spawn Timing")]
    [SerializeField] float minSpawnInterval = 10f, maxSpawnInterval = 30f;
    [Header("Accept Window")]
    [SerializeField] float acceptDuration = 20f;

    [Header("UI Prefabs & Parents")]
    [SerializeField] Transform availableContainer;
    [SerializeField] GameObject availableEntryPrefab;
    [SerializeField] Transform activeContainer;
    [SerializeField] GameObject activeEntryPrefab;


    // ---------


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetZones();
    }

    private void Start()
    {
        AcceptRandomJob();
        StartCoroutine(SpawnOffers());
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];
            job.UpdateTimer(dt);
            if (job.State == JobState.Expired)
            {
                if (debug) Debug.Log($"Job expired: {job.id}");
                activeJobs.RemoveAt(i);
                // TODO: Notify UI of expiration
            }
        }
    }

    private void SetZones()
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

    public void AcceptRandomJob()
    {
        if (pickupZones.Count == 0 || deliveryZones.Count == 0) return;

        var pickup = pickupZones[Random.Range(0, pickupZones.Count)];
        var dropoff = deliveryZones[Random.Range(0, deliveryZones.Count)];
        if (pickup == dropoff)
        {
            AcceptRandomJob();
            return;
        }

        var job = new DeliveryJob(
            System.Guid.NewGuid().ToString(),
            pickup,
            dropoff,
            Random.Range(minDeliveryTime, maxDeliveryTime)
        );
        activeJobs.Add(job);
        if (debug)
            Debug.Log($"Accepted job {job.id}: {pickup.zoneName} -> {dropoff.zoneName}");
        // TODO: Notify UI of new job
    }

    public void HandleZoneTrigger(DeliveryZone zone)
    {
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];

            // Pickup
            if (job.State == JobState.PendingPickup && job.pickupZone == zone)
            {
                job.Pickup();
                if (debug) Debug.Log($"Picked up job {job.id}");
                // TODO: Give package, update UI
            }
            // Dropoff
            else if (job.State == JobState.PendingDropoff && job.dropoffZone == zone)
            {
                job.Deliver();
                if (debug) Debug.Log($"Delivered job {job.id}");
                activeJobs.RemoveAt(i);
                // TODO: Reward player, update UI
            }
        }
    }

    public void UpdateZoneIndicators()
    {
        // pickups
        foreach (var zone in pickupZones)
        {
            bool show = activeJobs.Exists(j => j.State == JobState.PendingPickup && j.pickupZone == zone);
            zone.GetComponent<MeshRenderer>().enabled = show;
        }
        // dropoffs
        foreach (var zone in deliveryZones)
        {
            bool show = activeJobs.Exists(j => j.State == JobState.PendingDropoff && j.dropoffZone == zone);
            zone.GetComponent<MeshRenderer>().enabled = show;
        }
    }

    //Job Offers
    private IEnumerator SpawnOffers()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            SpawnOffer();
        }
    }

    void SpawnOffer()
    {
        // pick random pickup/dropoff zones
        DeliveryZone pu, doff;
        do
        {
            pu = pickupZones[Random.Range(0, pickupZones.Count)];
            doff = deliveryZones[Random.Range(0, deliveryZones.Count)];
        }
        // ensure not the same zone and not duplicated
        while (pu == doff
               || availableJobs.Exists(j => j.pickupZone == pu && j.dropoffZone == doff)
               || activeJobs.Exists(j => j.pickupZone == pu && j.dropoffZone == doff));

        // create the job
        var job = new DeliveryJob(Guid.NewGuid().ToString(), pu, doff, Random.Range(minDeliveryTime, maxDeliveryTime));
        availableJobs.Add(job);

        // instantiate UI entry
        var entryGO = Instantiate(availableEntryPrefab, availableContainer);
        var entry = entryGO.GetComponent<AvailableJobEntry>();
        entry.Setup(job, OnAccept, OnDeny);

        // auto-expire if untaken
        StartCoroutine(AutoExpire(job, entryGO));
    }

    IEnumerator AutoExpire(DeliveryJob job, GameObject entryGO)
    {
        yield return new WaitForSeconds(acceptDuration);
        if (availableJobs.Remove(job))
        {
            Destroy(entryGO);
            if (debug) Debug.Log($"Offer expired: {job.id}");
        }
    }

    void OnAccept(DeliveryJob job, GameObject entryGO)
    {
        availableJobs.Remove(job);
        Destroy(entryGO);

        activeJobs.Add(job);
        if (debug) Debug.Log($"Accepted {job.id}");
        UpdateZoneIndicators();

        // create an active-job UI entry
        var go = Instantiate(activeEntryPrefab, activeContainer);
        go.GetComponent<ActiveJobEntry>().Setup(job);
    }

    void OnDeny(DeliveryJob job, GameObject entryGO)
    {
        availableJobs.Remove(job);
        Destroy(entryGO);
        if (debug) Debug.Log($"Denied {job.id}");
    }

    //TODO: Build the “Offer” UI Prefab

    public IReadOnlyList<DeliveryJob> GetActiveJobs() => activeJobs;
}
