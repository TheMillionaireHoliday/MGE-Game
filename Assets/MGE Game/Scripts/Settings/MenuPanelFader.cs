using System;
using System.Collections;
using UnityEngine;

public class MenuPanelFader : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.12f;

    [Header("Easing")]
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup _group;
    private Coroutine _routine;

    public bool IsTransitioning { get; private set; }
    public bool IsVisible => _group.alpha > 0f;

    // Raised after the fade-in has fully completed and input is re-enabled.
    public event Action FadeInCompleted;
    // Raised after the fade-out has fully completed (panel is hidden).
    public event Action FadeOutCompleted;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        ApplyImmediate(visible: false);
    }

    // ---------- Public API ----------

    public void Show(Action onComplete = null)
    {
        StartTransition(FadeInCoroutine(onComplete));
    }

    public void Hide(Action onComplete = null)
    {
        StartTransition(FadeOutCoroutine(onComplete));
    }

    // Snap to a state with no animation and correct input state.
    public void SnapVisible()
    {
        StopRoutine();
        ApplyImmediate(visible: true);
    }

    public void SnapHidden()
    {
        StopRoutine();
        ApplyImmediate(visible: false);
    }

    // ---------- Internals ----------

    private void StartTransition(IEnumerator routine)
    {
        StopRoutine();
        _routine = StartCoroutine(routine);
    }

    private void StopRoutine()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        IsTransitioning = false;
    }

    public IEnumerator FadeInCoroutine(Action onComplete)
    {
        IsTransitioning = true;

        // Visible but non-interactive for the duration of the fade.
        _group.alpha = 1f;   // layout/raycast target still need to be on
        _group.blocksRaycasts = false;
        //_group.interactable   = false;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;   // menus usually run at timeScale 0
            float k = Mathf.Clamp01(t / fadeInDuration);
            _group.alpha = fadeInCurve.Evaluate(k);
            yield return null;
        }

        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        //_group.interactable   = true;

        IsTransitioning = false;
        _routine = null;

        onComplete?.Invoke();
        FadeInCompleted?.Invoke();
    }

    public IEnumerator FadeOutCoroutine(Action onComplete)
    {
        IsTransitioning = true;

        // Kill input immediately, even before alpha starts dropping.
        _group.blocksRaycasts = false;
        //_group.interactable   = false;

        float startAlpha = _group.alpha;
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOutDuration);
            _group.alpha = Mathf.Lerp(startAlpha, 0f, fadeOutCurve.Evaluate(k));
            yield return null;
        }

        _group.alpha = 0f;

        IsTransitioning = false;
        _routine = null;

        onComplete?.Invoke();
        FadeOutCompleted?.Invoke();
    }

    private void ApplyImmediate(bool visible)
    {
        _group.alpha = visible ? 1f : 0f;
        _group.blocksRaycasts = visible;
        //_group.interactable   = visible;
    }
}