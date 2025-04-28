using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // Максимальное здоровье
    public float currentHealth; // Текущее здоровье

    [Header("Stamina Settings")]
    public float maxStamina = 50f; // Максимальная выносливость
    public float currentStamina; // Текущая выносливость
    public float staminaDrainRate = 5f; // Скорость потери выносливости при беге
    public float staminaRecoveryRate = 3f; // Скорость восстановления выносливости
    public float staminaRecoveryDelay = 2f; // Задержка восстановления после использования

    private bool isRecoveringStamina = false;

    private void Start()
    {
        // Инициализация начальных значений
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        RecoverStamina(); // Восстанавливаем выносливость, если это возможно
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(); // Вызываем смерть при нулевом здоровье
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void UseStamina(float amount)
    {
        if (currentStamina > 0)
        {
            currentStamina -= amount;
            if (currentStamina < 0)
            {
                currentStamina = 0;
            }

            // Если выносливость израсходована, откладываем восстановление
            isRecoveringStamina = false;
            CancelInvoke(nameof(StartStaminaRecovery));
            Invoke(nameof(StartStaminaRecovery), staminaRecoveryDelay);
        }
    }
    
    // Количество изменения выносливости (положительное - восстановление, отрицательное - потеря)>
    public void ModifyStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // Если выносливость израсходована, откладываем восстановление
        if (amount < 0)
        {
            isRecoveringStamina = false;
            CancelInvoke(nameof(StartStaminaRecovery));
            Invoke(nameof(StartStaminaRecovery), staminaRecoveryDelay);
        }
    }

    private void RecoverStamina()
    {
        if (isRecoveringStamina && currentStamina < maxStamina)
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }
    }

    private void StartStaminaRecovery()
    {
        isRecoveringStamina = true;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Здесь можно добавить логику смерти, например, перезагрузку уровня
    }
}