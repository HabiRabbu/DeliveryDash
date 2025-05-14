using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    public static JobManager Instance;

    [SerializeField] private bool debug = false;

    [Header("Delivery Zones")]
    [SerializeField] private Transform pickupLocations;
    [SerializeField] private Transform deliveryLocations;

    [Header("Delivery Jobs")]
    public DeliveryJob currentJob; //TODO: MAKE THIS A LIST - MULTIPLE JOBS AT ONCE

    [Header("Tuning")]
    [SerializeField] private float minDeliveryTime = 60f;
    [SerializeField] private float maxDeliveryTime = 200f;

    private List<DeliveryZone> pickupZones;
    private List<DeliveryZone> deliveryZones;

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

    private void SetZones()
    {
        pickupZones = new List<DeliveryZone>();
        deliveryZones = new List<DeliveryZone>();

        foreach (Transform child in pickupLocations)
        {
            var zone = child.GetComponentInChildren<DeliveryZone>();
            if (zone != null && zone.zoneType == ZoneType.Pickup)
                pickupZones.Add(zone);
            else if (debug)
                Debug.LogWarning($"Missing or incorrect DeliveryZone on {child.name}");
        }

        foreach (Transform child in deliveryLocations)
        {
            var zone = child.GetComponentInChildren<DeliveryZone>();
            if (zone != null && zone.zoneType == ZoneType.Deliver)
                deliveryZones.Add(zone);
            else if (debug)
                Debug.LogWarning($"Missing or incorrect DeliveryZone on {child.name}");
        }

        if (debug)
            Debug.Log($"Found {pickupZones.Count} pickup zones and {deliveryZones.Count} delivery zones.");
    }

    private void Start()
    {
        AssignNewRandomJob();
    }

    public void AssignNewRandomJob()
    {
        if (pickupZones.Count == 0 || deliveryZones.Count == 0)
        {
            Debug.LogWarning("Zones not set correctly.");
            return;
        }

        var pickup = pickupZones[Random.Range(0, pickupZones.Count)];
        var dropoff = deliveryZones[Random.Range(0, deliveryZones.Count)];

        if (pickup == dropoff)
        {
            AssignNewRandomJob();
            return;
        }

        currentJob = new DeliveryJob(
            System.Guid.NewGuid().ToString(),
            pickup,
            dropoff,
            Random.Range(60f, 200f)
        );

        Debug.Log($"New Job: Pickup at {pickup.zoneName}, dropoff at {dropoff.zoneName}");
    }

    public void HandleZoneTrigger(DeliveryZone zone)
    {
        if (debug) { Debug.Log($"Player collided with {zone.zoneName}"); }

        if (currentJob == null || !currentJob.isActive)
        {
            // Only allow pickup at this point
            if (zone == currentJob.pickupZone)
            {
                currentJob.isActive = true;
                Debug.Log("Job started. Get to dropoff!");
                //TODO: Start timer
            }
        }
        else
        {
            if (zone == currentJob.dropoffZone)
            {
                Debug.Log("Job complete!");
                currentJob = null;
                AssignNewRandomJob();
            }
        }
        Debug.Log($"Pickup zone collider enabled: {currentJob.pickupZone.GetComponent<Collider>().enabled}");
        Debug.Log($"DeliveryZone component enabled: {currentJob.pickupZone.enabled}");
    }
}
