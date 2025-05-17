using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks all active (accepted) jobs, handles pickup & dropoff logic,
/// and drives the timer-based expiration (via UpdateTimer).
/// </summary>
public class JobManager : MonoBehaviour
{
    public static JobManager Instance { get; private set; }
    private readonly List<DeliveryJob> activeJobs = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        DeliveryEvents.OnJobAccepted += OnJobAccepted;
        DeliveryEvents.OnJobExpired += OnJobExpired;
    }
    void OnDisable()
    {
        DeliveryEvents.OnJobAccepted -= OnJobAccepted;
        DeliveryEvents.OnJobExpired -= OnJobExpired;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            activeJobs[i].UpdateTimer(dt);
            if (activeJobs[i].State == JobState.Expired)
            {
                activeJobs.RemoveAt(i);
            }
        }
    }

    private void OnJobAccepted(DeliveryJob job)
    {
        activeJobs.Add(job);
    }

    private void OnJobExpired(DeliveryJob job)
    {
        activeJobs.Remove(job);
    }

    /// <summary>Called by zones when the player enters them.</summary>
    public void HandleZoneTrigger(DeliveryZone zone)
    {
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];
            if (job.State == JobState.PendingPickup && job.pickupZone == zone)
            {
                job.Pickup();
            }
            else if (job.State == JobState.PendingDropoff && job.dropoffZone == zone)
            {
                job.Deliver();
                activeJobs.RemoveAt(i);
            }
        }
    }

    public IReadOnlyList<DeliveryJob> GetActiveJobs()
    {
        return activeJobs;
    }
}
