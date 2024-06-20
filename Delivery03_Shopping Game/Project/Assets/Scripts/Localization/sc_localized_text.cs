using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    public string textKey;
    private TMP_Text textValue;

    void Start()
    {
        textValue = GetComponent<TMP_Text>();
        SetText();
    }

    private void OnEnable()
    {
        Localizer.OnLanguageChangeDelegate += OnLanguageChanged;
    }

    private void OnDisable()
    {
        Localizer.OnLanguageChangeDelegate -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        SetText();
    }

    private void SetText()
    {
        textValue.text = Localizer.GetText(textKey);
    }
}
