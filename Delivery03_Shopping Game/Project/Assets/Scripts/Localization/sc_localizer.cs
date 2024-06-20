using System;
using System.Collections.Generic;
using UnityEngine;

public class Localizer : MonoBehaviour
{
    public static Localizer Instance;
    public Language DefaultLanguage;
    public TextAsset DataSheet;

    private Dictionary<string, LanguageData> Data;
    private Language _currentLanguage;
    
    public delegate void LanguageChangeDelegate();
    public static LanguageChangeDelegate OnLanguageChangeDelegate;

    private void Awake()
    {
        Instance = this;
        _currentLanguage = DefaultLanguage;
        LoadLanguageSheet();
    }

    public static string GetText(string textKey)
    {
        return Instance.Data[textKey].GetText(Instance._currentLanguage);
    }

    public static void SetLanguage(Language language)
    {
        Instance._currentLanguage = language;
        OnLanguageChangeDelegate?.Invoke();
    }

    void LoadLanguageSheet()
    {
        string[] lines = DataSheet.text.Split(new char[]{ '\n'});

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines.Length > 1) AddNewDataEntry(lines[i]);
        }
    }

    void AddNewDataEntry(string str)
    {
        string[] entry = str.Split(new char[] { ';' });

        var languageData = new LanguageData(entry);

        if (Data == null) Data = new Dictionary<string, LanguageData>();
        Data.Add(entry[0], languageData);
    }

    public static Language ChangeNextLanguage()
    {
        int nextLanguage = (int)Instance._currentLanguage + 1;

        if (nextLanguage > Enum.GetValues(typeof(Language)).Length)
        {
            nextLanguage = 1;
            Instance._currentLanguage = (Language)nextLanguage;
            Debug.Log("Next language: " + nextLanguage);
        }

        return (Language) nextLanguage;
    }

    public static string GetLanguage()
    {
        return Enum.GetName(typeof(Language), Instance._currentLanguage);
    }
}
