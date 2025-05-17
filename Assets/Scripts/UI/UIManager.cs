using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;

    [Header("Fading Popup Config")]
    [SerializeField] private GameObject fadingTextPopupPrefab;
    [SerializeField] private PopupMessageModel pickupMessage;
    [SerializeField] private PopupMessageModel deliveryMessage;

    private UIPrefabPool pool;

    void Awake()
    {
        pool = new UIPrefabPool(fadingTextPopupPrefab, canvasTransform);
    }

    void OnEnable()
    {
        DeliveryEvents.OnJobPickedUp += HandlePickup;
        DeliveryEvents.OnJobDelivered += HandleDelivery;
    }
    void OnDisable()
    {
        DeliveryEvents.OnJobPickedUp -= HandlePickup;
        DeliveryEvents.OnJobDelivered -= HandleDelivery;
    }


    public void HandlePickup(DeliveryJob job)
    {
        ShowCentrePopup(pickupMessage);
    }

    public void HandleDelivery(DeliveryJob job)
    {
        ShowCentrePopup(deliveryMessage);
    }

    private void ShowCentrePopup(PopupMessageModel msg)
    {
        var go = pool.Get();
        var popup = go.GetComponent<FadingPopupText>();
        popup.Show(msg, () => pool.Release(go));
    }
}
