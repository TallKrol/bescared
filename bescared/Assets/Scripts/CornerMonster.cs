using UnityEngine;

public class CornerMonster : MonoBehaviour
{
    [Header("Monster Settings")]
    public int damage = 50; // Урон, который монстр наносит игроку
    public float rotationSpeed = 5f; // Скорость поворота к игроку
    public float damageRadius = 2f; // Радиус, на котором монстр наносит урон
    public LayerMask wallLayer; // Слой стен для проверки видимости

    [Header("References")]
    public Transform player; // Ссылка на игрока

    private Transform roomTransform; // Ссылка на комнату, в которой находится монстр
    private bool isInitialized = false;
    private Vector3 spawnPosition; // Позиция спавна монстра
    private float lastDamageTime; // Время последнего нанесения урона
    private float damageCooldown = 1f; // Кулдаун между нанесением урона

    private void Start()
    {
        spawnPosition = transform.position; // Сохраняем позицию спавна
        FindAndSetupRoom();
    }

    private void FindAndSetupRoom()
    {
        // Ищем ближайшую комнату
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, 5f);
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
            Debug.LogError("Не удалось найти комнату для углового монстра!");
        }
    }

    private void SetupRoom(Transform room)
    {
        roomTransform = room;
        isInitialized = true;
        Debug.Log($"Угловой монстр привязан к комнате: {room.name}");
    }

    private void Update()
    {
        if (!isInitialized || player == null) return;

        // Поворачиваемся к игроку
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Игнорируем вертикальную составляющую
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Проверяем, находится ли игрок в радиусе урона и нет ли стены между ними
        if (Vector3.Distance(transform.position, player.position) <= damageRadius && 
            CanSeePlayer())
        {
            DealDamage();
        }
    }

    private bool CanSeePlayer()
    {
        // Проверяем, нет ли стены между монстром и игроком
        Vector3 directionToPlayer = player.position - transform.position;
        return !Physics.Raycast(transform.position, directionToPlayer.normalized, 
            directionToPlayer.magnitude, wallLayer);
    }

    private void DealDamage()
    {
        // Проверяем кулдаун
        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    // Вызывается при попадании в монстра
    public void OnHit()
    {
        DealDamage();
    }

    private void OnDrawGizmos()
    {
        // Визуализация радиуса урона в редакторе
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);

        // Визуализация луча проверки видимости
        if (player != null)
        {
            Gizmos.color = CanSeePlayer() ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
} 