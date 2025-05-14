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


    public IReadOnlyList<DeliveryJob> GetActiveJobs() => activeJobs;
}
