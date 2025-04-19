using UnityEngine;
using UnityEngine.AI;

public class TwistedPillow : MonoBehaviour
{
    [Header("Pillow Settings")]
    public int health = 10; // Здоровье подушки
    public float moveSpeed = 3f; // Скорость передвижения
    public int damage = 10; // Урон, если подушка касается игрока

    [Header("References")]
    public Transform player; // Ссылка на игрока

    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
        }
    }

    private void Update()
    {
        if (player != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        Debug.Log("Подушка уничтожена!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }
    }
}