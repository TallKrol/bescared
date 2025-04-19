using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // ������������ ��������
    public float currentHealth; // ������� ��������

    [Header("Stamina Settings")]
    public float maxStamina = 50f; // ������������ �������
    public float currentStamina; // ������� �������
    public float staminaDrainRate = 5f; // �������� ���������� ������� ��� ����
    public float staminaRecoveryRate = 3f; // �������� �������������� �������
    public float staminaRecoveryDelay = 2f; // �������� ����� ��������������� ����� �������������

    private bool isRecoveringStamina = false;

    private void Start()
    {
        // ������������� ��������� ��������
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        RecoverStamina(); // ��������������� �������, ���� ��� ��������
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die(); // �������� ����� ������ ������
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

            // ���� ������� ������������, ���������� ��������������
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
        // ����� ����� �������� ������ ��� ������, ��������, ���������� ������
    }
}