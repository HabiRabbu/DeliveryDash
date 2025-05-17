using UnityEngine;

public enum ZoneType { Pickup, Deliver }

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    public ZoneType zoneType;
    public string zoneName = "placeholder";

    [SerializeField] private MeshRenderer indicatorRenderer;

    void Awake()
    {
        if (indicatorRenderer != null)
            indicatorRenderer.enabled = false;
        else
            Debug.LogWarning($"[{name}] no indicatorRenderer assigned.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        JobManager.Instance.HandleZoneTrigger(this);
    }

    /// <summary>Called by ZoneIndicatorController to show or hide the mesh.</summary>
    public void SetIndicator(bool show)
    {
        if (indicatorRenderer != null)
            indicatorRenderer.enabled = show;
    }

    void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = zoneType == ZoneType.Pickup ? Color.green : Color.blue;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
