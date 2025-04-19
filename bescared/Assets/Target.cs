using UnityEngine;

public class Target : MonoBehaviour
{
    public int health = 50; // Здоровье объекта

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
        Destroy(gameObject); // Удаляем объект
    }
}