using System;
using UnityEngine;

public enum JobState { PendingPickup, PendingDropoff, Completed, Expired }

[Serializable]
public class DeliveryJob
{
    public string id;
    public DeliveryZone pickupZone;
    public DeliveryZone dropoffZone;
    public float timeLimit;
    public float timeRemaining;
    public JobState State { get; private set; }

    public DeliveryJob(string id, DeliveryZone pickup, DeliveryZone dropoff, float timeLimit)
    {
        this.id = id;
        this.pickupZone = pickup;
        this.dropoffZone = dropoff;
        this.timeLimit = timeLimit;
        this.timeRemaining = timeLimit;
        this.State = JobState.PendingPickup;
    }

    public void UpdateTimer(float delta)
    {
        if (State == JobState.Completed || State == JobState.Expired) return;
        timeRemaining -= delta;
        if (timeRemaining <= 0f)
        {
            State = JobState.Expired;
        }
    }

    public void Pickup()
    {
        if (State != JobState.PendingPickup) return;
        State = JobState.PendingDropoff;
    }

    public void Deliver()
    {
        if (State != JobState.PendingDropoff) return;
        State = JobState.Completed;
    }
}
