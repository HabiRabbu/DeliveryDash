using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class AvailableJobEntry : MonoBehaviour {
    [SerializeField] TMP_Text pickupTxt, dropoffTxt, timerTxt;
    [SerializeField] Button acceptBtn, denyBtn;

    DeliveryJob job;
    System.Action<DeliveryJob, GameObject> onAccept, onDeny;

    public void Setup(DeliveryJob j, System.Action<DeliveryJob, GameObject> accept, System.Action<DeliveryJob, GameObject> deny) {
        job = j;
        onAccept = accept;
        onDeny = deny;

        pickupTxt.text = $"Pickup: {job.pickupZone.zoneName}";
        dropoffTxt.text = $"Dropoff: {job.dropoffZone.zoneName}";

        acceptBtn.onClick.AddListener(() => onAccept(job, gameObject));
        denyBtn.onClick.AddListener(() => onDeny(job, gameObject));

        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer() {
        while (true) {
            timerTxt.text = $"Expires in: {Mathf.Ceil(job.timeRemaining)}s";
            yield return null;
        }
    }
}
