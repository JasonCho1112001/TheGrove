using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverSequence : MonoBehaviour
{
    [Header("Game Over Elements")]
    public GameObject gameOverX;
    public GameObject errText;

    [Header("Note Elements")]
    public GameObject noteGroup;
    public TMP_Text tipText;

    [Header("Reset Elements")]
    public GameObject resetGroup;
    public TMP_Text resetNumber;

    [Header("Tips")]
        string[] tips = new string[]
    {
        "Stay away from the monster!",
        "Check the monster's eyes! If they're red, switch lanes!",
        "Conserve your stamina!",
        "Watch out for obstacles!",
        "Stay close to your group!",
        "Silence can keep you alive."
    };

    void Start()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        noteGroup.SetActive(false);
        resetGroup.SetActive(false);

        // Show Game Over elements
        gameOverX.SetActive(true);
        errText.SetActive(true);

        yield return new WaitForSeconds(2f);

        // Show note section
        noteGroup.SetActive(true);

        // Pick random tip
        tipText.text = tips[Random.Range(0, tips.Length)];

        yield return new WaitForSeconds(3f);

        // Show reset section
        resetGroup.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            resetNumber.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // Load title screen
        SceneManager.LoadScene("Title Screen");
    }
}
