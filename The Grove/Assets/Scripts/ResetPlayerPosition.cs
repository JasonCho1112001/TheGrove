using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPlayerPosition : MonoBehaviour
{
    
    // Audio source for complete level
    //[Header("Finish Audio Settings")]
    //[SerializeField] public AudioSource finishAudioSource;
    //[SerializeField] public AudioClip finishAudioClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

/*  //Originally, I wanted where it plays a sound and then resets the player position, but then I pivot to just simply resetting the scene.
    public void Start()
    {
       
        if (finishAudioSource != null)
        {
            Debug.Log("Finish Audio Source is not assigned in the inspector.");
        }
        if (finishAudioClip == null)
        {
            Debug.Log("Finish Audio Clip is not assigned in the inspector.");
        }
        
    }
*/
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player finished level, resetting position.");
            SceneManager.LoadScene("Simple Level Terrain");
        }
        if (other.CompareTag("Friend"))
        {
            Debug.Log("Friend finished level, deleting friend.");
            Destroy(other.gameObject);
        }
    }
}
