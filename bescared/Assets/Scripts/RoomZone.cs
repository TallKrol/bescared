using UnityEngine;
using System.Collections;

public class RoomZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public RoomZoneData zoneData; // Данные зоны
    public bool isActive = true; // Активна ли зона

    [Header("Настройки шейдера")]
    [Tooltip("Шейдер, который будет применяться при входе в зону")]
    public Shader zoneShader;
    [Tooltip("Материал с шейдером для плавного перехода")]
    public Material zoneMaterial;
    [Tooltip("Время перехода шейдера (в секундах)")]
    public float shaderTransitionTime = 1f;
    [Tooltip("Максимальная интенсивность эффекта шейдера")]
    public float maxShaderIntensity = 1f;

    private Collider zoneCollider; // Коллайдер зоны
    private ParticleSystem particles; // Ссылка на партиклы
    private SanitySystem playerSanity; // Ссылка на систему рассудка игрока
    private PlayerStats playerStats; // Ссылка на статистику игрока
    private CharacterController playerController; // Ссылка на контроллер игрока
    private float defaultGravity; // Стандартная гравитация
    private bool isPlayerInZone = false;
    private Coroutine shaderTransitionCoroutine;
    private static readonly string SHADER_INTENSITY_PROPERTY = "_EffectIntensity";

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

        if (zoneMaterial != null && zoneShader != null)
        {
            // Инициализируем материал с шейдером
            zoneMaterial = new Material(zoneShader);
            // Устанавливаем начальную интенсивность в 0
            zoneMaterial.SetFloat(SHADER_INTENSITY_PROPERTY, 0f);
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

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
    {
            isPlayerInZone = true;
            OnPlayerEnter();
            
            // Запускаем плавное включение шейдера
            if (zoneMaterial != null)
            {
                if (shaderTransitionCoroutine != null)
        {
                    StopCoroutine(shaderTransitionCoroutine);
                }
                shaderTransitionCoroutine = StartCoroutine(TransitionShaderIntensity(0f, maxShaderIntensity));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            OnPlayerExit();
            
            // Запускаем плавное выключение шейдера
            if (zoneMaterial != null)
            {
                if (shaderTransitionCoroutine != null)
                {
                    StopCoroutine(shaderTransitionCoroutine);
                }
                shaderTransitionCoroutine = StartCoroutine(TransitionShaderIntensity(maxShaderIntensity, 0f));
            }
        }
    }

    private IEnumerator TransitionShaderIntensity(float startValue, float endValue)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < shaderTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shaderTransitionTime;
            
            // Используем плавную интерполяцию
            float currentIntensity = Mathf.Lerp(startValue, endValue, t);
            zoneMaterial.SetFloat(SHADER_INTENSITY_PROPERTY, currentIntensity);
            
            yield return null;
        }
        
        // Устанавливаем финальное значение
        zoneMaterial.SetFloat(SHADER_INTENSITY_PROPERTY, endValue);
    }

    protected virtual void OnPlayerEnter()
    {
        // Базовый метод для переопределения в наследниках
        Debug.Log($"Игрок вошел в зону: {zoneData.roomId}");
    }

    protected virtual void OnPlayerExit()
    {
        // Базовый метод для переопределения в наследниках
        Debug.Log($"Игрок вышел из зоны: {zoneData.roomId}");
    }

    private void OnDestroy()
    {
        // Очищаем материал при уничтожении объекта
        if (zoneMaterial != null)
        {
            Destroy(zoneMaterial);
        }
    }

    // Визуализация зоны в редакторе
    private void OnDrawGizmos()
    {
        if (!isActive) return;

        Gizmos.color = isPlayerInZone ? Color.red : Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
} 