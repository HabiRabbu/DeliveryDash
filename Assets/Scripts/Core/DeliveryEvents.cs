using System;

/// <summary>
/// Central event bus for all job-related signals.  
/// Subscribers register in OnEnable, unregister in OnDisable.
/// </summary>
public static class DeliveryEvents
{
    public static event Action<DeliveryJob> OnOfferCreated;
    public static event Action<DeliveryJob> OnOfferExpired;
    public static event Action<DeliveryJob> OnJobAccepted;
    public static event Action<DeliveryJob> OnJobPickedUp;
    public static event Action<DeliveryJob> OnJobDelivered;
    public static event Action<DeliveryJob> OnJobExpired;

    //Requires and returns DeliveryJob
    public static void OfferCreated(DeliveryJob j) => OnOfferCreated?.Invoke(j);
    public static void OfferExpired(DeliveryJob j)  => OnOfferExpired?.Invoke(j);
    public static void JobAccepted(DeliveryJob j)  => OnJobAccepted?.Invoke(j);
    public static void JobPickedUp(DeliveryJob j)  => OnJobPickedUp?.Invoke(j);
    public static void JobDelivered(DeliveryJob j) => OnJobDelivered?.Invoke(j);
    public static void JobExpired(DeliveryJob j)   => OnJobExpired?.Invoke(j);
}
