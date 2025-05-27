using UnityEngine;

public class TargetWithLight : MonoBehaviour, IDamageable
{
    public int health = 50; // Здоровье мишени

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void TakeLightDamage(float amount)
    {
        health -= Mathf.RoundToInt(amount);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " was destroyed!");
        Destroy(gameObject); // Уничтожаем мишень
    }
}