using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterPrefab; // Префаб монстра
    public int minMonsters = 3; // Минимальное количество монстров
    public int maxMonsters = 8; // Максимальное количество монстров
    public Transform[] spawnPoints; // Точки спавна монстров

    private bool monstersSpawned = false;

    public void SpawnMonsters()
    {
        if (monstersSpawned) return; // Избегаем повторного спавна
        monstersSpawned = true;

        int monsterCount = Random.Range(minMonsters, maxMonsters);

        for (int i = 0; i < monsterCount; i++)
        {
            // Выбираем случайную точку спавна в комнате
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

            // Проверяем, находится ли монстр на NavMesh
            NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isOnNavMesh)
            {
                Debug.LogWarning($"Монстр {monster.name} спавнится вне NavMesh! Перемещение отменено.");
                Destroy(monster); // Удаляем монстра, если он вне NavMesh
            }
        }
    }
}