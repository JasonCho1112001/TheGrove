using UnityEngine;

public class hideOnStart : MonoBehaviour
{
    public bool hide = true;
    public bool hideChildren = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(hide && GetComponent<MeshRenderer>() != null)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }

        if (hideChildren)
        {
            foreach (MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
