using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    [Header("Настройки рассудка")]
    public float maxSanity = 100f; // Максимальное значение рассудка
    public float minSanity = -25f; // Минимальное значение рассудка
    public float currentSanity; // Текущее значение рассудка
    public float sanityDrainRate = 5f; // Скорость потери рассудка (в темноте, в секунду)
    public float sanityRecoveryRate = 3f; // Скорость восстановления рассудка (на свету, в секунду)

    [Header("Проверка света")]
    public LayerMask lightLayer; // Слой света для проверки
    public float lightCheckRadius = 5f; // Радиус проверки наличия света вокруг игрока

    private bool isInLight = false; // Находится ли игрок в зоне света

    private void Start()
    {
        // Инициализация начального значения рассудка
        currentSanity = maxSanity;
    }

    private void Update()
    {
        CheckLight();

        if (isInLight)
        {
            RecoverSanity();
        }
        else
        {
            DrainSanity();
        }

        // Ограничение значения рассудка от -25 до 100
        currentSanity = Mathf.Clamp(currentSanity, minSanity, maxSanity);
    }

    private void CheckLight()
    {
        // Проверяем, находится ли игрок в зоне света
        Collider[] lightSources = Physics.OverlapSphere(transform.position, lightCheckRadius, lightLayer);
        isInLight = lightSources.Length > 0;
    }

    private void DrainSanity()
    {
        // Потеря рассудка
        currentSanity -= sanityDrainRate * Time.deltaTime;
    }

    private void RecoverSanity()
    {
        // Восстановление рассудка
        currentSanity += sanityRecoveryRate * Time.deltaTime;
    }

    /// <summary>
    /// Изменяет значение рассудка на указанное количество
    /// </summary>
    /// <param name="amount">Количество изменения рассудка (положительное - восстановление, отрицательное - потеря)</param>
    public void ModifySanity(float amount)
    {
        currentSanity += amount;
    }
}
