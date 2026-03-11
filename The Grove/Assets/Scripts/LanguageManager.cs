using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    private bool isChangingLanguage = false;

    // Call this from your UI Buttons or Dropdown!
    // 0 = English, 1 = Spanish, 2 = Japanese (based on the order you generated them)
    public void ChangeLanguage(int localeID)
    {
        if (isChangingLanguage) return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int localeID)
    {
        isChangingLanguage = true;

        // Wait for the localization system to initialize just in case
        yield return LocalizationSettings.InitializationOperation;

        // Swap the active language
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];

        isChangingLanguage = false;
        Debug.Log("Language switched to: " + LocalizationSettings.SelectedLocale.LocaleName);
    }
}