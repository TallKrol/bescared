using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class TwistedPillow : MonoBehaviour, IDamageable
{
    [Header("Pillow Settings")]
    public int health = 10; // Количество здоровья подушки
    public float moveSpeed = 3f; // Скорость передвижения
    public int damage = 10; // Урон, который подушка наносит игроку
    public float roomDetectionRadius = 5f; // Радиус поиска комнаты

    [Header("References")]
    public Transform player; // Ссылка на игрока

    private NavMeshAgent navMeshAgent;
    private Transform roomTransform; // Ссылка на комнату, в которой находится подушка
    private NavMeshSurface roomNavMesh; // NavMesh поверхности комнаты
    private bool isInitialized = false;
    private Vector3 spawnPosition; // Позиция спавна подушки

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position; // Сохраняем позицию спавна
        FindAndSetupRoom();
    }

    private void FindAndSetupRoom()
    {
        // Ищем ближайшую комнату
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, roomDetectionRadius);
        Transform closestRoom = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Room"))
            {
                float distance = Vector3.Distance(spawnPosition, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRoom = collider.transform;
                }
            }
        }

        if (closestRoom != null)
        {
            SetupRoom(closestRoom);
        }
        else
        {
            Debug.LogError("Не удалось найти комнату для подушки!");
        }
    }

    private void SetupRoom(Transform room)
    {
        roomTransform = room;
        
        // Проверяем, есть ли уже NavMeshSurface
        roomNavMesh = roomTransform.GetComponent<NavMeshSurface>();
        if (roomNavMesh == null)
        {
            // Если нет, создаём новый
            roomNavMesh = roomTransform.gameObject.AddComponent<NavMeshSurface>();
            roomNavMesh.collectObjects = CollectObjects.Volume;
            roomNavMesh.agentTypeID = 0; // Humanoid
        }
        
        // Строим NavMesh
        roomNavMesh.BuildNavMesh();
        
        // Настраиваем NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.autoTraverseOffMeshLink = true;
            navMeshAgent.autoRepath = true;
            navMeshAgent.areaMask = roomNavMesh.defaultArea;
        }
        
        isInitialized = true;
        Debug.Log($"Подушка привязана к комнате: {room.name}");
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (player != null && navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            // Проверяем, находится ли игрок в той же комнате
            if (IsPlayerInRoom())
        {
            navMeshAgent.SetDestination(player.position);
        }
            else
            {
                // Если игрок не в комнате, возвращаемся в центр комнаты
                navMeshAgent.SetDestination(roomTransform.position);
            }
        }
    }

    private bool IsPlayerInRoom()
    {
        if (roomTransform == null) return false;

        // Проверяем, находится ли игрок в пределах комнаты
        Collider roomCollider = roomTransform.GetComponent<Collider>();
        if (roomCollider == null) return false;

        Bounds roomBounds = roomCollider.bounds;
        return roomBounds.Contains(player.position);
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

    private void OnDrawGizmos()
    {
        // Визуализация радиуса поиска комнаты в редакторе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, roomDetectionRadius);
    }
}