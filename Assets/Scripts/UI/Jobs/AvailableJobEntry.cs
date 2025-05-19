using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Represents one “offer” in the UI.  
/// Calls back into DeliveryEvents when Accept/Deny clicked.
/// </summary>
public class AvailableJobEntry : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] TMP_Text pickupTxt, dropoffTxt, timerTxt;
    [SerializeField] Image acceptBtn, denyBtn;

    [Header("Scheme Sprites")]
    [SerializeField] Sprite acceptBtn_Xbox, declineBtn_Xbox;
    [SerializeField] Sprite acceptBtn_Ps, declineBtn_Ps;
    [SerializeField] Sprite acceptBtn_Mkb, declineBtn_Mkb;

    public string jobId { get; private set; }

    private DeliveryJob job;
    private System.Action<DeliveryJob, GameObject> onAccept, onDeny;

    public void Setup(DeliveryJob j,
                      System.Action<DeliveryJob, GameObject> accept,
                      System.Action<DeliveryJob, GameObject> deny)
    {
        job = j;
        jobId = j.id;
        onAccept = accept;
        onDeny = deny;

        pickupTxt.text = $"Pickup: {j.pickupZone.zoneName}";
        dropoffTxt.text = $"Dropoff: {j.dropoffZone.zoneName}";

        setupButtons();
    }

    void Update()
    {
        timerTxt.text = $"Expires: {Mathf.Ceil(job.timeRemaining)}s";
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Accept()
    {
        onAccept?.Invoke(job, gameObject);
    }

    public void Decline()
    {
        onDeny?.Invoke(job, gameObject);
    }

    private void setupButtons()
    {
        var scheme = getCurrentScheme();

        switch (scheme)
        {
            case controlSchemeType.Xbox:
                acceptBtn.sprite = acceptBtn_Xbox;
                denyBtn.sprite = declineBtn_Xbox;
                break;
            case controlSchemeType.PlayStation:
                acceptBtn.sprite = acceptBtn_Ps;
                denyBtn.sprite = declineBtn_Ps;
                break;
            default:
                acceptBtn.sprite = acceptBtn_Mkb;
                denyBtn.sprite = declineBtn_Mkb;
                break;
        }
    }

    private enum controlSchemeType { KeyboardMouse, Xbox, PlayStation, Unknown }
    private controlSchemeType getCurrentScheme()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) { return controlSchemeType.Unknown; }

        if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad) { return controlSchemeType.PlayStation; }
        if (gamepad is UnityEngine.InputSystem.DualShock.DualSenseGamepadHID) { return controlSchemeType.PlayStation; }

        if (gamepad is UnityEngine.InputSystem.XInput.XInputController) { return controlSchemeType.Xbox; }

        return controlSchemeType.Unknown;
    }
}
