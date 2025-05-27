using UnityEngine;

public class Target : MonoBehaviour, IDamageable
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

    private void Die()
    {
        Debug.Log(gameObject.name + " was destroyed!");
        Destroy(gameObject); // Уничтожаем мишень
    }
}