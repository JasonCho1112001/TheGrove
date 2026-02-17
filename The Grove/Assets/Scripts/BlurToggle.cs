using System.Collections;
using UnityEngine;
using Unified.UniversalBlur.Runtime; // <-- from the package
using UnityEngine.InputSystem;

public class BlurToggle : MonoBehaviour
{
    [SerializeField] private UniversalBlurFeature blurFeature;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.B;
    [SerializeField, Min(0.01f)] private float transitionSeconds = 0.35f;
    [SerializeField, Range(0f, 1f)] private float blurredIntensity = 1f; // 1 = full blur, 0 = none

    private Coroutine _tween;
    private bool _isBlurred;

    private void Awake()
    {
        if (blurFeature != null)
            blurFeature.Intensity = 0f; // start clear
    }

    private void Update()
    {
        if (blurFeature == null) return;
        if (Keyboard.current == null) return;

        // Convert KeyCode → InputSystem Key
        Key inputKey = (Key)System.Enum.Parse(typeof(Key), toggleKey.ToString());

        if (Keyboard.current[inputKey].wasPressedThisFrame)
        {
            _isBlurred = !_isBlurred;
            float target = _isBlurred ? blurredIntensity : 0f;

            if (_tween != null) StopCoroutine(_tween);
            _tween = StartCoroutine(AnimateIntensity(target, transitionSeconds));
        }
    }

    private IEnumerator AnimateIntensity(float target, float seconds)
    {
        float start = blurFeature.Intensity;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / seconds; // unscaled so it still animates if you pause time
            float eased = SmoothStep(t);
            blurFeature.Intensity = Mathf.Lerp(start, target, eased);
            yield return null;
        }

        blurFeature.Intensity = target;
        _tween = null;
    }

    private static float SmoothStep(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}