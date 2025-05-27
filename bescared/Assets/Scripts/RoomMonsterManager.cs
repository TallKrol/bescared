using UnityEngine;
using System.Collections.Generic;

public class RoomMonsterManager : MonoBehaviour
{
    public static RoomMonsterManager Instance { get; private set; }

    [Header("Настройки монстров")]
    [Tooltip("Количество монстров, которое нужно убить для получения крови")]
    public int requiredKillsForBlood = 3;
    [Tooltip("Текущее количество убитых монстров")]
    public int currentKills = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Вызывается при убийстве монстра
    /// </summary>
    public void OnMonsterKilled()
    {
        currentKills++;
        Debug.Log($"Монстр убит. Прогресс: {currentKills}/{requiredKillsForBlood}");

        if (currentKills >= requiredKillsForBlood)
        {
            // Даем игроку кровь
            PlayerInventory.Instance.AddBlood();
            // Сбрасываем счетчик
            currentKills = 0;
            Debug.Log("Получена бутылка крови!");
        }
    }

    /// <summary>
    /// Сбрасывает счетчик убитых монстров
    /// </summary>
    public void ResetKills()
    {
        currentKills = 0;
    }
} 