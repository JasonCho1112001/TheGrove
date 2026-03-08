using UnityEngine;

public class TutorialTriggerUI : MonoBehaviour
{
    [Header("Tutorial UI Settings")]
    [SerializeField] public GameObject tutorialUIShow;

    public void Start()
    {
        if (tutorialUIShow != null)
        {
            tutorialUIShow.SetActive(false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialUIShow != null)
            {
                tutorialUIShow.SetActive(true);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialUIShow != null)
            {
                tutorialUIShow.SetActive(false);
            }
        }
    }
}