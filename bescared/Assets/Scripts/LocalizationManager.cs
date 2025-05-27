using UnityEngine;
using System.Collections.Generic;
using System;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [SerializeField] private TextAsset[] languageFiles;
    [SerializeField] private string defaultLanguage = "en";

    private Dictionary<string, Dictionary<string, string>> localizationData = new Dictionary<string, Dictionary<string, string>>();
    private string currentLanguage;

    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLocalization();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLocalization()
    {
        currentLanguage = PlayerPrefs.GetString("SelectedLanguage", defaultLanguage);
        LoadAllLanguages();
    }

    private void LoadAllLanguages()
    {
        foreach (var languageFile in languageFiles)
        {
            if (languageFile == null) continue;

            try
            {
                var languageData = JsonUtility.FromJson<LanguageData>(languageFile.text);
                if (languageData != null)
                {
                    var translations = new Dictionary<string, string>();
                    foreach (var entry in languageData.entries)
                    {
                        translations[entry.key] = entry.value;
                    }
                    localizationData[languageData.languageCode] = translations;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading language file {languageFile.name}: {e.Message}");
            }
        }
    }

    public void SetLanguage(string languageCode)
    {
        if (localizationData.ContainsKey(languageCode))
        {
            currentLanguage = languageCode;
            PlayerPrefs.SetString("SelectedLanguage", languageCode);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Language {languageCode} not found!");
        }
    }

    public string GetTranslation(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;

        if (localizationData.TryGetValue(currentLanguage, out var translations))
        {
            if (translations.TryGetValue(key, out var translation))
            {
                return translation;
            }
        }

        // Если перевод не найден, возвращаем ключ
        Debug.LogWarning($"Translation not found for key: {key} in language: {currentLanguage}");
        return key;
    }

    public string GetTranslation(string key, params object[] args)
    {
        var translation = GetTranslation(key);
        try
        {
            return string.Format(translation, args);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error formatting translation for key {key}: {e.Message}");
            return translation;
        }
    }

    public List<string> GetAvailableLanguages()
    {
        return new List<string>(localizationData.Keys);
    }

    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }
}

[Serializable]
public class LanguageData
{
    public string languageCode;
    public List<TranslationEntry> entries;
}

[Serializable]
public class TranslationEntry
{
    public string key;
    public string value;
} 