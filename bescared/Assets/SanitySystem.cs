using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f; // Максимальный уровень рассудка
    public float minSanity = -25f; // Минимальный уровень рассудка
    public float currentSanity; // Текущий уровень рассудка
    public float sanityDrainRate = 5f; // Скорость уменьшения рассудка (например, в темноте)
    public float sanityRecoveryRate = 3f; // Скорость восстановления рассудка (например, на свету)

    [Header("Light Check")]
    public LayerMask lightLayer; // Слой света для проверки
    public float lightCheckRadius = 5f; // Радиус проверки света вокруг игрока

    private bool isInLight = false; // Находится ли игрок в зоне света

    private void Start()
    {
        // Устанавливаем начальное значение рассудка
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

        // Ограничиваем значение рассудка в пределах от -25 до 100
        currentSanity = Mathf.Clamp(currentSanity, minSanity, maxSanity);
    }

    private void CheckLight()
    {
        // Проверка, находится ли игрок в зоне света
        Collider[] lightSources = Physics.OverlapSphere(transform.position, lightCheckRadius, lightLayer);
        isInLight = lightSources.Length > 0;
    }

    private void DrainSanity()
    {
        // Уменьшение рассудка
        currentSanity -= sanityDrainRate * Time.deltaTime;
    }

    private void RecoverSanity()
    {
        // Восстановление рассудка
        currentSanity += sanityRecoveryRate * Time.deltaTime;
    }

    public void TakeSanityDamage(float amount)
    {
        // Уменьшение рассудка от внешних факторов
        currentSanity -= amount;
    }

    public void RestoreSanity(float amount)
    {
        // Восстановление рассудка от внешних факторов
        currentSanity += amount;
    }
}
