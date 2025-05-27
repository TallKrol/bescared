using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Система инвентаря игрока
/// 
/// Структура инвентаря:
/// - Сетка 6x4 (6 колонок, 4 ряда)
/// - Ряд D (нижний) является хотбаром
/// - Позиции в хотбаре:
///   - 1D: Огнестрельное оружие
///   - 2D: Фонарик
///   - 6D: Бутылка крови
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Настройки инвентаря")]
    public const int INVENTORY_WIDTH = 6;
    public const int INVENTORY_HEIGHT = 4;
    public const int HOTBAR_ROW = 3; // D ряд (0-based)

    [Header("Хотбар")]
    public const int WEAPON_SLOT = 0;
    public const int FLASHLIGHT_SLOT = 1;
    public const int BLOOD_SLOT = 5;

    [Header("UI")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Transform slotsParent;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI ammoText;

    [Header("Ресурсы")]
    public int gold = 0;
    public int ammo = 0;
    public int maxAmmo = 100;

    private InventorySlot[,] inventoryGrid;
    private bool isInventoryOpen = false;
    private HashSet<string> keys = new HashSet<string>();
    private int bloodCount = 0;

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
        InitializeInventory();
        LoadKeys();
        LoadBlood();
        UpdateUI();
    }

    private void Update()
    {
        // Открытие/закрытие инвентаря по клавише I
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void InitializeInventory()
    {
        inventoryGrid = new InventorySlot[INVENTORY_WIDTH, INVENTORY_HEIGHT];

        // Создаем слоты инвентаря
        for (int y = 0; y < INVENTORY_HEIGHT; y++)
        {
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsParent);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                
                slot.slotIndex = y * INVENTORY_WIDTH + x;
                slot.isHotbarSlot = (y == HOTBAR_ROW);
                
                inventoryGrid[x, y] = slot;
            }
        }

        // Скрываем инвентарь при старте
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

    /// <summary>
    /// Добавляет ключ в инвентарь игрока
    /// </summary>
    /// <param name="keyId">Уникальный идентификатор ключа</param>
    public void AddKey(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return;
        
        keys.Add(keyId);
        Debug.Log($"Добавлен ключ: {keyId}");
        SaveKeys();
    }

    /// <summary>
    /// Удаляет ключ из инвентаря игрока
    /// </summary>
    /// <param name="keyId">Уникальный идентификатор ключа</param>
    public void RemoveKey(string keyId)
    {
        if (string.IsNullOrEmpty(keyId)) return;

        if (keys.Remove(keyId))
        {
            Debug.Log($"Удален ключ: {keyId}");
            SaveKeys();
        }
    }

    /// <summary>
    /// Проверяет наличие ключа у игрока
    /// </summary>
    /// <param name="keyId">Уникальный идентификатор ключа</param>
    /// <returns>True если ключ есть в инвентаре</returns>
    public static bool HasKey(string keyId)
    {
        if (Instance == null) return false;
        return Instance.keys.Contains(keyId);
    }

    /// <summary>
    /// Возвращает список всех ключей в инвентаре
    /// </summary>
    /// <returns>Массив идентификаторов ключей</returns>
    public string[] GetAllKeys()
    {
        string[] keyArray = new string[keys.Count];
        keys.CopyTo(keyArray);
        return keyArray;
    }

    /// <summary>
    /// Сохраняет состояние ключей в PlayerPrefs
    /// </summary>
    private void SaveKeys()
    {
        string keysString = string.Join(",", keys);
        PlayerPrefs.SetString("PlayerKeys", keysString);
        PlayerPrefs.Save();
        Debug.Log("Инвентарь сохранен");
    }

    /// <summary>
    /// Загружает состояние ключей из PlayerPrefs
    /// </summary>
    private void LoadKeys()
    {
        if (PlayerPrefs.HasKey("PlayerKeys"))
        {
            string keysString = PlayerPrefs.GetString("PlayerKeys");
            if (!string.IsNullOrEmpty(keysString))
            {
                string[] keyArray = keysString.Split(',');
                keys = new HashSet<string>(keyArray);
                Debug.Log($"Загружено {keys.Count} ключей");
            }
        }
    }

    /// <summary>
    /// Добавляет кровь в инвентарь
    /// </summary>
    /// <param name="amount">Количество крови</param>
    public void AddBlood(int amount = 1)
    {
        bloodCount += amount;
        Debug.Log($"Добавлена кровь. Текущее количество: {bloodCount}");
        SaveBlood();
    }

    /// <summary>
    /// Использует кровь из инвентаря
    /// </summary>
    /// <returns>True если кровь была использована</returns>
    public bool UseBlood()
    {
        if (bloodCount > 0)
        {
            bloodCount--;
            Debug.Log($"Использована кровь. Осталось: {bloodCount}");
            SaveBlood();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Проверяет наличие крови в инвентаре
    /// </summary>
    /// <returns>True если есть хотя бы одна бутылка крови</returns>
    public static bool HasBlood()
    {
        if (Instance == null) return false;
        return Instance.bloodCount > 0;
    }

    /// <summary>
    /// Возвращает текущее количество крови
    /// </summary>
    public static int GetBloodCount()
    {
        if (Instance == null) return 0;
        return Instance.bloodCount;
    }

    private void SaveBlood()
    {
        PlayerPrefs.SetInt("PlayerBloodCount", bloodCount);
        PlayerPrefs.Save();
    }

    private void LoadBlood()
    {
        bloodCount = PlayerPrefs.GetInt("PlayerBloodCount", 0);
        Debug.Log($"Загружено {bloodCount} бутылок крови");
    }

    /// <summary>
    /// Очищает весь инвентарь
    /// </summary>
    public void ClearInventory()
    {
        keys.Clear();
        SaveKeys();
        Debug.Log("Инвентарь очищен");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void AddAmmo(int amount)
    {
        ammo = Mathf.Min(ammo + amount, maxAmmo);
        UpdateUI();
    }

    public bool SpendAmmo(int amount)
    {
        if (ammo >= amount)
        {
            ammo -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    public bool HasEnoughAmmo(int amount)
    {
        return ammo >= amount;
    }

    public bool AddItem(Item item)
    {
        // Сначала пытаемся добавить в существующий стак
        if (item.stackable)
        {
            for (int y = 0; y < INVENTORY_HEIGHT; y++)
            {
                for (int x = 0; x < INVENTORY_WIDTH; x++)
                {
                    InventorySlot slot = inventoryGrid[x, y];
                    if (slot.currentItem != null && 
                        slot.currentItem.itemName == item.itemName && 
                        slot.currentItem.count < slot.currentItem.maxStack)
                    {
                        slot.currentItem.count += item.count;
                        slot.UpdateVisuals();
                        return true;
                    }
                }
            }
        }

        // Если не удалось добавить в существующий стак, ищем пустой слот
        for (int y = 0; y < INVENTORY_HEIGHT; y++)
        {
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                InventorySlot slot = inventoryGrid[x, y];
                if (slot.currentItem == null)
                {
                    slot.SetItem(item);
                    return true;
                }
            }
        }

        return false; // Инвентарь полон
    }

    public void RemoveItem(int x, int y)
    {
        if (x >= 0 && x < INVENTORY_WIDTH && y >= 0 && y < INVENTORY_HEIGHT)
        {
            inventoryGrid[x, y].ClearSlot();
        }
    }

    public Item GetItem(int x, int y)
    {
        if (x >= 0 && x < INVENTORY_WIDTH && y >= 0 && y < INVENTORY_HEIGHT)
        {
            return inventoryGrid[x, y].currentItem;
        }
        return null;
    }

    private void UpdateUI()
    {
        if (goldText != null)
            goldText.text = $"Золото: {gold}";
        if (ammoText != null)
            ammoText.text = $"Патроны: {ammo}/{maxAmmo}";
    }
} 