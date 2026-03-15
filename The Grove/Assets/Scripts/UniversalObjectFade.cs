using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UniversalObjectFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeInDuration = 3f;
    public float fadeOutDuration = 3f;

    [Header("Fading Objects")]
    [SerializeField] private CanvasGroup canvasGrp;
    [SerializeField] private Graphic[] uiGraphics;
    [SerializeField] private Renderer[] meshRenderers;

    // Read only alpha values
    private readonly float fullAlpha = 1f;
    private readonly float clearAlpha = 0f;

    public void FadeOut()
    {
        
        StartCoroutine(Fade(fullAlpha, clearAlpha, fadeOutDuration));
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(clearAlpha, fullAlpha, fadeInDuration));
    }

    IEnumerator Fade(float startingAlpha, float endAlpha, float fadeDuration)
    {
        float time = 0f;

        // Grabs alpha value during duration
        while (time < fadeDuration)
        {
            float alphaVal = Mathf.Lerp(startingAlpha, endAlpha, time / fadeDuration);
            SetObjectAlpha(alphaVal);

            time += Time.deltaTime;

            yield return null;
        }

        // Double check to have Object's alpha with chosen end value
        SetObjectAlpha(endAlpha);
    }

    private void SetObjectAlpha(float alphaVal)
    {
        if (canvasGrp != null)
        {
            canvasGrp.alpha = alphaVal;
        }

        // Grabs the array of uiGraphics and sets the alpha and retains original color
        foreach (var graphic in uiGraphics)
        {
            if (graphic == null) continue;

            Color graphicColor = graphic.color;
            graphicColor.a = alphaVal;
            graphic.color = graphicColor;
        }

        // Grabs the array of meshRenderers
        foreach (var renderer in meshRenderers)
        {
            if (renderer == null) continue;

            // Sets the alpha and retains original color on every material in renderer
            foreach (var material in renderer.materials)
            {
                Color matColor = material.color;
                matColor.a = alphaVal;
                material.color = matColor;
            }
        }
    }
}
