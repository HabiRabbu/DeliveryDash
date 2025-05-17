using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents one “offer” in the UI.  
/// Calls back into DeliveryEvents when Accept/Deny clicked.
/// </summary>
public class AvailableJobEntry : MonoBehaviour
{
    [SerializeField] TMP_Text pickupTxt, dropoffTxt, timerTxt;
    [SerializeField] Button  acceptBtn, denyBtn;

    DeliveryJob job;
    System.Action<DeliveryJob,GameObject> onAccept, onDeny;

    public void Setup(DeliveryJob j,
                      System.Action<DeliveryJob,GameObject> accept,
                      System.Action<DeliveryJob,GameObject> deny)
    {
        job = j;
        onAccept = accept;
        onDeny = deny;

        pickupTxt.text  = $"Pickup: {j.pickupZone.zoneName}";
        dropoffTxt.text = $"Dropoff: {j.dropoffZone.zoneName}";

        acceptBtn.onClick.AddListener(() => onAccept(job, gameObject));
        denyBtn. onClick.AddListener(() => onDeny (job, gameObject));
    }

    void Update()
    {
        timerTxt.text = $"Expires: {Mathf.Ceil(job.timeRemaining)}s";
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
