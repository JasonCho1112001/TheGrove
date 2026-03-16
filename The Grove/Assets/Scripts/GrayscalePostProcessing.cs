using UnityEngine;

public class GrayscalePostProcessing : MonoBehaviour
{
    public Shader myShader;
    private Material myMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMaterial = new Material(myShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, myMaterial);
    }
}
