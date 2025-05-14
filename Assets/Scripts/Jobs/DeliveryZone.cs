using UnityEngine;

public enum ZoneType { Pickup, Dropoff }

public class DeliveryZone : MonoBehaviour {
    public ZoneType zoneType;
    public string zoneName;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        JobManager.Instance.HandleZoneTrigger(this);
    }
}
