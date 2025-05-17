using TMPro;
using UnityEngine;

public class ActiveJobEntry : MonoBehaviour {
    [SerializeField] TMP_Text routeTxt, timerTxt;
    DeliveryJob job;

    public void Setup(DeliveryJob j) {
        job = j;
        routeTxt.text = $"{job.pickupZone.zoneName} → {job.dropoffZone.zoneName}";
    }

    void Update() {
        if (job.State == JobState.Expired || job.State == JobState.Completed) {
            return;
        }

        timerTxt.text = $"Time Left: {Mathf.Ceil(job.timeRemaining)}s";
    }
}