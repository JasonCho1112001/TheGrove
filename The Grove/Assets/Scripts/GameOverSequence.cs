using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization;

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

[Header("Localized Tips")]
    // This creates a list in the Inspector where you can pick your translations
    public LocalizedString[] localizedTips;

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

        // Pick a random string and get its translated text
        if (localizedTips.Length > 0)
        {
            var randomTip = localizedTips[Random.Range(0, localizedTips.Length)];
            tipText.text = randomTip.GetLocalizedString();
        }

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
