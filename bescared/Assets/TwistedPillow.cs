using UnityEngine;
using UnityEngine.AI;

public class TwistedPillow : MonoBehaviour
{
    [Header("Pillow Settings")]
    public int health = 10; // �������� �������
    public float moveSpeed = 3f; // �������� ������������
    public int damage = 10; // ����, ���� ������� �������� ������

    [Header("References")]
    public Transform player; // ������ �� ������

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
        Debug.Log("������� ����������!");
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