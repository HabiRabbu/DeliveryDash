using System.Linq;
using UnityEngine;

public class ZoneIndicatorController : MonoBehaviour
{
    [Header("Zone Roots (assign as above)")]
    [SerializeField] private Transform pickupLocationsRoot;
    [SerializeField] private Transform deliveryLocationsRoot;

    private DeliveryZone[] allZones;

    void Awake()
    {
        // Combine both sets of zones into one array
        var pu = pickupLocationsRoot.GetComponentsInChildren<DeliveryZone>();
        var doff = deliveryLocationsRoot.GetComponentsInChildren<DeliveryZone>();
        allZones = pu.Concat(doff).ToArray();
    }

    void OnEnable()
    {
        DeliveryEvents.OnOfferCreated   += Refresh;
        DeliveryEvents.OnOfferExpired   += Refresh;
        DeliveryEvents.OnJobAccepted    += Refresh;
        DeliveryEvents.OnJobPickedUp    += Refresh;
        DeliveryEvents.OnJobDelivered   += Refresh;
        DeliveryEvents.OnJobExpired     += Refresh;
    }

    void OnDisable()
    {
        DeliveryEvents.OnOfferCreated   -= Refresh;
        DeliveryEvents.OnOfferExpired   -= Refresh;
        DeliveryEvents.OnJobAccepted    -= Refresh;
        DeliveryEvents.OnJobPickedUp    -= Refresh;
        DeliveryEvents.OnJobDelivered   -= Refresh;
        DeliveryEvents.OnJobExpired     -= Refresh;
    }

    /// <summary>
    /// Toggle each zone’s indicator based on whether there’s any active job
    /// that needs pickup or dropoff there.
    /// </summary>
    void Refresh(DeliveryJob _ = null)
    {
        var jobs = JobManager.Instance.GetActiveJobs();

        foreach (var zone in allZones)
        {
            bool wantPickup = jobs.Any(j =>
                j.State == JobState.PendingPickup && j.pickupZone == zone);

            bool wantDropoff = jobs.Any(j =>
                j.State == JobState.PendingDropoff && j.dropoffZone == zone);

            zone.SetIndicator(wantPickup || wantDropoff);
        }
    }
}
