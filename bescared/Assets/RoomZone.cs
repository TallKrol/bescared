using UnityEngine;

public class RoomZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public RoomZoneData zoneData; // Данные зоны
    public bool isActive = true; // Активна ли зона

    private Collider zoneCollider; // Коллайдер зоны
    private ParticleSystem particles; // Ссылка на партиклы
    private SanitySystem playerSanity; // Ссылка на систему рассудка игрока
    private PlayerStats playerStats; // Ссылка на статистику игрока
    private CharacterController playerController; // Ссылка на контроллер игрока
    private float defaultGravity; // Стандартная гравитация

    private void Start()
    {
        // Получаем компоненты
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
        {
            Debug.LogError("RoomZone requires a Collider component!");
            return;
        }

        // Настраиваем партиклы, если они есть
        if (zoneData.zoneParticles != null)
        {
            particles = Instantiate(zoneData.zoneParticles, transform);
            particles.transform.localPosition = Vector3.zero;
            particles.gameObject.SetActive(isActive);
        }

        // Находим компоненты игрока
        playerSanity = FindObjectOfType<SanitySystem>();
        playerStats = FindObjectOfType<PlayerStats>();
        playerController = FindObjectOfType<CharacterController>();

        if (playerSanity == null)
        {
            Debug.LogWarning("SanitySystem not found in scene!");
        }
        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not found in scene!");
        }
        if (playerController == null)
        {
            Debug.LogWarning("CharacterController not found in scene!");
        }
        else
        {
            // Сохраняем стандартную гравитацию
            defaultGravity = Physics.gravity.y;
        }
    }

    private void Update()
    {
        if (!isActive) return;

        // Проверяем, находится ли игрок в зоне
        if (IsPlayerInZone())
        {
            // Применяем эффекты зоны
            ApplyZoneEffects();
        }
        else
        {
            // Сбрасываем эффекты, если игрок вышел из зоны
            ResetZoneEffects();
        }
    }

    private bool IsPlayerInZone()
    {
        // Проверяем, находится ли игрок в коллайдере зоны
        return zoneCollider.bounds.Contains(playerController.transform.position);
    }

    private void ApplyZoneEffects()
    {
        // Применяем потерю рассудка
        if (playerSanity != null)
        {
            playerSanity.ModifySanity(-zoneData.sanityDrainRate * Time.deltaTime);
        }

        // Применяем потерю стамины
        if (playerStats != null)
        {
            playerStats.ModifyStamina(-zoneData.staminaDrainRate * Time.deltaTime);
        }

        // Применяем туман
        RenderSettings.fog = true;
        RenderSettings.fogDensity = zoneData.fogDensity;
        RenderSettings.fogColor = zoneData.fogColor;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogEndDistance = zoneData.fogDistance;

        // Применяем гравитацию
        if (playerController != null)
        {
            Physics.gravity = new Vector3(0, defaultGravity * zoneData.gravityMultiplier, 0);
        }
    }

    private void ResetZoneEffects()
    {
        // Сбрасываем туман
        RenderSettings.fog = false;

        // Сбрасываем гравитацию
        if (playerController != null)
        {
            Physics.gravity = new Vector3(0, defaultGravity, 0);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (particles != null)
        {
            particles.gameObject.SetActive(active);
        }

        if (!active)
        {
            ResetZoneEffects();
        }
    }

    private void OnDrawGizmos()
    {
        // Визуализация зоны в редакторе
        if (zoneCollider != null)
        {
            Gizmos.color = new Color(zoneData.fogColor.r, zoneData.fogColor.g, zoneData.fogColor.b, 0.3f);
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);
        }
    }
} 