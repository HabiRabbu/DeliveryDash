using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class PhoneController : MonoBehaviour
{
    private PlayerControls controls;

    [Header("Slide Positions")]
    [Tooltip("Position when phone is hidden")]
    public Vector2 hiddenAnchoredPos;
    [Tooltip("Position when phone is fully in view")]
    public Vector2 visibleAnchoredPos;

    [Header("Slide Duration")]
    public float slideDuration = 0.5f;

    private RectTransform rt;

    void Awake()
    {
        controls = new PlayerControls();
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = hiddenAnchoredPos;
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.DisplayPhone.performed += _ => ShowPhone(true);
        controls.UI.DisplayPhone.canceled += _ => ShowPhone(false);
    }

    private void OnDisable()
    {
        controls.UI.DisplayPhone.performed -= _ => ShowPhone(true);
        controls.UI.DisplayPhone.canceled -= _ => ShowPhone(false);
        controls.UI.Disable();
    }

    void ShowPhone(bool open)
    {
        rt.DOKill();
        var target = open ? visibleAnchoredPos : hiddenAnchoredPos;
        var ease = open ? Ease.OutBack : Ease.InBack;
        if (open) Debug.Log("LB Pressed!");
        rt.DOAnchorPos(target, slideDuration).SetEase(ease);
    }
}
