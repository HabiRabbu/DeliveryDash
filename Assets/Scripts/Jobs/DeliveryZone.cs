using UnityEngine;

public enum ZoneType { Pickup, Deliver }

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    public ZoneType zoneType;
    public string zoneName = "placeholder";
    MeshRenderer rend;

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        rend.enabled = false;  // start invisible
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        JobManager.Instance.HandleZoneTrigger(this);
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = zoneType == ZoneType.Pickup ? Color.green : Color.blue;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }

    void OnEnable()
    {
        JobManager.Instance.UpdateZoneIndicators(); // refresh on load
    }
}

//TODO: Organise with this at some point?

// private void OnValidate() {
//     if (zoneType == ZoneType.Pickup)
//         gameObject.name = $"PickupZone_{zoneName}";
//     else
//         gameObject.name = $"DropoffZone_{zoneName}";
// }