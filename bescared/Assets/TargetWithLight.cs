using UnityEngine;

public class TargetWithLight : MonoBehaviour
{
    public int health = 50; // Здоровье объекта

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
        Debug.Log(gameObject.name + " was destroyed by light!");
        Destroy(gameObject); // Удаляем объект
    }
}