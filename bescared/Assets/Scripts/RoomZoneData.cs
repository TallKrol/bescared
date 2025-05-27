using UnityEngine;

[CreateAssetMenu(fileName = "NewZone", menuName = "Room Zone Data")]
public class RoomZoneData : ScriptableObject
{
    [Header("Основные настройки")]
    [Tooltip("Название зоны")]
    public string zoneName = "Новая зона";

    [Header("Эффекты рассудка")]
    [Tooltip("Скорость потери рассудка в зоне")]
    public float sanityDrainRate = 5f;

    [Header("Эффекты выносливости")]
    [Tooltip("Скорость потери выносливости в зоне")]
    public float staminaDrainRate = 3f;

    [Header("Настройки тумана")]
    [Tooltip("Плотность тумана (0 - нет тумана)")]
    [Range(0f, 1f)]
    public float fogDensity = 0.1f;

    [Tooltip("Цвет тумана")]
    public Color fogColor = Color.black;

    [Tooltip("Дальность тумана")]
    [Min(0f)]
    public float fogDistance = 20f;

    [Header("Физика")]
    [Tooltip("Множитель гравитации (1 - нормальная гравитация)")]
    [Min(0f)]
    public float gravityMultiplier = 1f;

    [Header("Визуальные эффекты")]
    [Tooltip("Партиклы зоны (опционально)")]
    public ParticleSystem zoneParticles;

    [Header("ID зоны/комнаты")]
    public string roomId;
} 