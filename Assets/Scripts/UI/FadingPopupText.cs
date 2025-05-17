using TMPro;
using UnityEngine;
using System;
using System.Collections;

public class FadingPopupText : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private TMP_Text tmpText;
    private Coroutine fadeRoutine;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 1, 1, 0);

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        tmpText = GetComponentInChildren<TMP_Text>();
    }

    public void Show(PopupMessageModel msg, Action onComplete)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        canvasGroup.alpha = 1f;

        tmpText.text = msg.text;
        tmpText.color = msg.color;

        fadeRoutine = StartCoroutine(FadeOut(msg.fadeDuration, onComplete));
    }

    private IEnumerator FadeOut(float duration, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tNorm = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Clamp01(fadeCurve.Evaluate(tNorm));
            yield return null;
        }

        canvasGroup.alpha = 0f;

        onComplete?.Invoke();
    }
}
