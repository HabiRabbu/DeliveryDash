using UnityEngine;

public enum ZoneType { Pickup, Deliver }

public class DeliveryZone : MonoBehaviour
{
    public ZoneType zoneType;
    public string zoneName = "placeholder"; //TODO: If it's a delivery location, pick a random name from a json list (e.g. Kelly, Fritz, Robert) - "Deliver to Robert"

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Player collided with {zoneName} - BEFORE CompareTag check");
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player collided with {zoneName}");
        JobManager.Instance.HandleZoneTrigger(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log($"Exited {zoneName}");
    }
}


//TODO: Organise with this at some point?

// private void OnValidate() {
//     if (zoneType == ZoneType.Pickup)
//         gameObject.name = $"PickupZone_{zoneName}";
//     else
//         gameObject.name = $"DropoffZone_{zoneName}";
// }