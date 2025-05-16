using UnityEngine;

public enum ZoneType { Pickup, Deliver }

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    public ZoneType zoneType;
    public string zoneName = "placeholder";

    [SerializeField] private MeshRenderer indicatorRenderer;

    private void Awake()
    {
        if (indicatorRenderer != null)
            indicatorRenderer.enabled = false;
        else
            Debug.LogWarning($"DeliveryZone '{gameObject.name}' has no indicatorRenderer assigned.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        JobManager.Instance.HandleZoneTrigger(this);
    }

    /// <summary>
    /// Enable or disable the visible indicator mesh.
    /// </summary>
    public void SetIndicator(bool show)
    {
        if (indicatorRenderer != null)
            indicatorRenderer.enabled = show;
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
}