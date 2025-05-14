using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    public static JobManager Instance;

    [Header("Delivery Zones")]
    [SerializeField] public List<DeliveryZone> pickupZones;
    [SerializeField] public List<DeliveryZone> dropoffZones;

    [Header("Delivery Jobs")]
    public DeliveryJob currentJob; //TODO: MAKE THIS A LIST - MULTIPLE JOBS AT ONCE

    [Header("Tuning")]
    [SerializeField] private float minDeliveryTime = 60f;
    [SerializeField] private float maxDeliveryTime = 200f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        AssignNewRandomJob();
    }

    public void AssignNewRandomJob()
    {
        if (pickupZones.Count == 0 || dropoffZones.Count == 0)
        {
            Debug.LogWarning("Zones not set correctly.");
            return;
        }

        var pickup = pickupZones[Random.Range(0, pickupZones.Count)];
        var dropoff = dropoffZones[Random.Range(0, dropoffZones.Count)];

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
    }
}
