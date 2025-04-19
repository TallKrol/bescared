using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // Максимальное здоровье
    public float currentHealth; // Текущее здоровье

    [Header("Stamina Settings")]
    public float maxStamina = 50f; // Максимальная стамина
    public float currentStamina; // Текущая стамина
    public float staminaDrainRate = 5f; // Скорость уменьшения стамины при беге
    public float staminaRecoveryRate = 3f; // Скорость восстановления стамины
    public float staminaRecoveryDelay = 2f; // Задержка перед восстановлением после использования

    private bool isRecoveringStamina = false;

    private void Start()
    {
        // Устанавливаем стартовые значения
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        RecoverStamina(); // Восстанавливаем стамину, если это возможно
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(); // Вызываем метод смерти игрока
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

            // Если стамина используется, сбрасываем восстановление
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
        // Здесь можно добавить логику для смерти, например, перезапуск уровня
    }
}