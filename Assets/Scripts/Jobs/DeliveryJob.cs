using UnityEngine;

public class DeliveryJob {
    public string id;
    public DeliveryZone pickupZone;
    public DeliveryZone dropoffZone;
    public float timeLimit;
    public bool isActive;

    public DeliveryJob(string id, DeliveryZone pickup, DeliveryZone dropoff, float timeLimit) {
        this.id = id;
        pickupZone = pickup;
        dropoffZone = dropoff;
        this.timeLimit = timeLimit;
        isActive = false;
    }
}
