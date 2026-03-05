using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenOpener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fadeOverlay;      // Black overlay image
    [SerializeField] private CanvasGroup camcorderUI;
    [SerializeField] private CanvasGroup titleGroup;

    [Header("Timing")]
    [SerializeField] private float fadeOutSeconds = 1f;

    private void Awake()
    {
        // Start hidden
        camcorderUI.alpha = 0f;
        titleGroup.alpha = 0f;

        // Start with black overlay visible
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0,0,0,1);
        }
    }

    private void Start()
    {
        StartCoroutine(OpenTitle());
    }

    private IEnumerator OpenTitle()
    {
        // Fade black overlay out
        yield return FadeOverlay();

        // Instantly show UI
        camcorderUI.alpha = 1f;

        // Instantly show title
        titleGroup.alpha = 2f;
    }

    private IEnumerator FadeOverlay()
    {
        if (fadeOverlay == null)
            yield break;

        float t = 0f;
        Color c = fadeOverlay.color;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeOutSeconds;
            c.a = Mathf.Lerp(1f, 0f, t);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeOverlay.color = c;
    }
}