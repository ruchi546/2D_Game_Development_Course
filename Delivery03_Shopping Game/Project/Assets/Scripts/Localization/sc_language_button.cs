using UnityEngine;

public class LanguageButton : MonoBehaviour
{
    public Language Language;

    [SerializeField] private AudioClip selectSound;

    public void HandleInputData(int languageIndex)
    {
        SoundManager.Instance.PlaySound(selectSound);
        
        if (languageIndex == 0)
        {
            Localizer.SetLanguage(Language.English);
        }
        else if (languageIndex == 1)
        {
            Localizer.SetLanguage(Language.Catalan);
        }
        else if (languageIndex == 2)
        {
            Localizer.SetLanguage(Language.Spanish);
        }
    }
}
