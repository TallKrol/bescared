using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class MonsterBook : MonoBehaviour
{
    public static MonsterBook Instance { get; private set; }

    [SerializeField] private List<BookEntry> allEntries = new List<BookEntry>();
    private bool isInInventory = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadProgress();
    }

    public void OnMonsterEncountered(BookEntry entry)
    {
        if (!isInInventory || entry == null)
        {
            Debug.LogWarning($"Cannot encounter monster: {(entry == null ? "Entry is null" : "Book is not in inventory")}");
            return;
        }

        entry.Discover();
        SaveProgress();
    }

    public void OnDeathByMonster(BookEntry entry)
    {
        if (!isInInventory || entry == null)
        {
            Debug.LogWarning($"Cannot record death: {(entry == null ? "Entry is null" : "Book is not in inventory")}");
            return;
        }

        entry.Study();
        SaveProgress();
    }

    public List<BookEntry> GetEntriesInDiscoveryOrder()
    {
        if (allEntries == null)
        {
            Debug.LogError("Entries list is null!");
            return new List<BookEntry>();
        }

        return allEntries.Where(e => e != null && e.IsDiscovered)
                        .OrderBy(e => e.DiscoveryTime)
                        .ToList();
    }

    public void SetInInventory(bool value)
    {
        isInInventory = value;
        Debug.Log($"Book {(value ? "added to" : "removed from")} inventory");
    }

    private void SaveProgress()
    {
        try
        {
            if (allEntries == null)
            {
                Debug.LogError("Cannot save progress: entries list is null!");
                return;
            }

            foreach (var entry in allEntries)
            {
                if (entry == null) continue;

                string key = $"MonsterBook_{entry.name}";
                PlayerPrefs.SetInt($"{key}_Discovered", entry.IsDiscovered ? 1 : 0);
                PlayerPrefs.SetInt($"{key}_Studied", entry.IsStudied ? 1 : 0);
                PlayerPrefs.SetInt($"{key}_DeathCount", entry.DeathCount);
                PlayerPrefs.SetFloat($"{key}_DiscoveryTime", entry.DiscoveryTime);
            }
            PlayerPrefs.Save();
            Debug.Log("Progress saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving progress: {e.Message}");
        }
    }

    private void LoadProgress()
    {
        try
        {
            if (allEntries == null)
            {
                Debug.LogError("Cannot load progress: entries list is null!");
                return;
            }

            foreach (var entry in allEntries)
            {
                if (entry == null) continue;

                string key = $"MonsterBook_{entry.name}";
                if (PlayerPrefs.HasKey($"{key}_Discovered"))
                {
                    if (PlayerPrefs.GetInt($"{key}_Discovered") == 1)
                    {
                        entry.Discover();
                    }
                    if (PlayerPrefs.GetInt($"{key}_Studied") == 1)
                    {
                        entry.Study();
                    }
                }
            }
            Debug.Log("Progress loaded successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading progress: {e.Message}");
        }
    }

    public void ResetProgress()
    {
        try
        {
            if (allEntries == null)
            {
                Debug.LogError("Cannot reset progress: entries list is null!");
                return;
            }

            foreach (var entry in allEntries)
            {
                if (entry == null) continue;
                entry.Reset();
            }
            PlayerPrefs.DeleteAll();
            Debug.Log("Progress reset successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error resetting progress: {e.Message}");
        }
    }
} 