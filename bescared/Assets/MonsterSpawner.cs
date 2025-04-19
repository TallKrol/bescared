using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterPrefab; // ������ �������
    public int minMonsters = 3; // ����������� ���������� ��������
    public int maxMonsters = 8; // ������������ ���������� ��������
    public Transform[] spawnPoints; // ����� ������ ��������

    private bool monstersSpawned = false;

    public void SpawnMonsters()
    {
        if (monstersSpawned) return; // �������� ���������� ������
        monstersSpawned = true;

        int monsterCount = Random.Range(minMonsters, maxMonsters);

        for (int i = 0; i < monsterCount; i++)
        {
            // �������� ��������� ����� ������ � �������
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

            // ���������, ��������� �� ������ �� NavMesh
            NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null && !agent.isOnNavMesh)
            {
                Debug.LogWarning($"������ {monster.name} ��������� ��� NavMesh! ����������� ��������.");
                Destroy(monster); // ������� �������, ���� �� ��� NavMesh
            }
        }
    }
}