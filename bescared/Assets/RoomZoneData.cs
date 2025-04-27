using UnityEngine;

[System.Serializable]
public class RoomZoneData
{
    [Header("Zone Settings")]
    public string zoneName; // Название зоны
    public float fogDensity; // Плотность тумана (0 - нет тумана)
    public float sanityDrainRate; // Скорость потери рассудка в зоне
    public Color fogColor; // Цвет тумана
    public float fogDistance; // Дальность тумана
    public ParticleSystem zoneParticles; // Партиклы зоны (опционально)

    [Header("Player Effects")]
    public float staminaDrainRate; // Скорость потери стамины в зоне
    public float gravityMultiplier = 1f; // Множитель гравитации (1 - нормальная гравитация)

    public RoomZoneData(string name, float density, float sanityDrain, Color color, float distance, 
        float staminaDrain = 0f, float gravityMult = 1f, ParticleSystem particles = null)
    {
        zoneName = name;
        fogDensity = Mathf.Clamp01(density);
        sanityDrainRate = sanityDrain;
        fogColor = color;
        fogDistance = Mathf.Max(0f, distance);
        staminaDrainRate = staminaDrain;
        gravityMultiplier = Mathf.Max(0f, gravityMult);
        zoneParticles = particles;
    }
} 