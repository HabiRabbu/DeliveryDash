using System;

/// <summary>
/// Data + state for a single delivery job.
/// Exposes methods to transition state and fires events.
/// </summary>
public enum JobState { PendingPickup, PendingDropoff, Completed, Expired }

public class DeliveryJob
{
    public readonly string id;
    public readonly DeliveryZone pickupZone;
    public readonly DeliveryZone dropoffZone;
    public readonly float timeLimit;  

    public float timeRemaining { get; private set; }
    public JobState State { get; private set; }

    public DeliveryJob(string id, DeliveryZone pu, DeliveryZone doff, float timeLimit)
    {
        this.id            = id;
        this.pickupZone    = pu;
        this.dropoffZone   = doff;
        this.timeLimit     = timeLimit;
        this.timeRemaining = timeLimit;
        State = JobState.PendingPickup;
    }

    /// <summary>Called every frame by JobManager to decrement the timer.</summary>
    public void UpdateTimer(float dt)
    {
        if (State == JobState.PendingPickup || State == JobState.PendingDropoff)
        {
            timeRemaining -= dt;
            if (timeRemaining <= 0f)
            {
                State = JobState.Expired;
                DeliveryEvents.JobExpired(this);
            }
        }
    }

    public void Pickup()
    {
        if (State != JobState.PendingPickup) return;
        State = JobState.PendingDropoff;
        DeliveryEvents.JobPickedUp(this);
    }

    public void Deliver()
    {
        if (State != JobState.PendingDropoff) return;
        State = JobState.Completed;
        DeliveryEvents.JobDelivered(this);
    }
}
