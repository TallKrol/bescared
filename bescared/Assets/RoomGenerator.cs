using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class RoomGenerator : MonoBehaviour
{
    [Header("Standard Rooms")]
    public GameObject firstRoomPrefab; // ������ ��� ������ �������
    public GameObject[] roomPrefabs; // ������ ������� �������� ������
    public float[] prefabWeights; // ������ ����� ��� ������ ������� ������

    [Header("Special Rooms")]
    public GameObject[] nightmareRoomPrefabs; // ������ �������� ��������� ������
    public float nightmareRoomWeight = 0.2f; // ����������� ����� ��� ��������� ������
    public GameObject[] peacefulRoomPrefabs; // ������ �������� ������ ������
    public float peacefulRoomWeight = 0.8f; // ����������� ����� ��� ������ ������

    [Header("Interior")]
    public GameObject[] interiorPrefabs; // ������ �������� ��� ������
    public float[] interiorWeights; // ������ ����� ��� ������ ������
    public GameObject chandelierPrefab; // ������ ��� ������
    public GameObject windowPrefab; // ������ ��� ����
    public GameObject picturePrefab; // ������ ��� �������

    [Header("Monsters")]
    public GameObject monsterPrefab; // ������ �������
    public int minMonsters = 5; // ����������� ���������� ��������
    public int maxMonsters = 15; // ������������ ���������� ��������

    [Header("Player and Sanity")]
    public Transform player; // ������ �� ������ ������
    public SanitySystem sanitySystem; // ������� �������� ������

    [Header("Generation Settings")]
    public float generationDistance = 20f; // ���������� ��� ��������� ����� ������
    public float deactivationDistance = 30f; // ���������� ��� ����������� ������
    public int width = 5; // ������ ��������� ����� ������
    public int height = 5; // ������ ��������� ����� ������
    public float wallThickness = 1f; // ������� ����

    private Vector3 lastPlayerPosition;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>();

    private void Start()
    {
        lastPlayerPosition = player.position;

        // ��������� ������ �������
        GenerateFirstRoom();

        // ��������� ��������� ��������� ������
        GenerateInitialRooms();
    }

    private void Update()
    {
        if (Vector3.Distance(player.position, lastPlayerPosition) > generationDistance)
        {
            lastPlayerPosition = player.position;
            GenerateRoomsAroundPlayer();
        }

        DeactivateRoomsFarFromPlayer();
    }

    private void GenerateFirstRoom()
    {
        if (firstRoomPrefab == null)
        {
            Debug.LogError("������ ������ ������� �� ��������!");
            return;
        }

        // ������ ������ ������� � ������������� �������
        Vector2 firstRoomPosition = Vector2.zero;
        GameObject firstRoom = Instantiate(firstRoomPrefab, new Vector3(firstRoomPosition.x, 0, firstRoomPosition.y), Quaternion.identity);
        generatedRooms[firstRoomPosition] = firstRoom;

        // ��������� ������ � ������ �������
        GenerateInteriorObjects(firstRoom);
        GenerateSpecialObjects(firstRoom);

        // ������������� NavMesh ��� ������ �������
        BuildRoomNavMesh(firstRoom);
    }

    private void GenerateInitialRooms()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 roomPosition = new Vector2(x * 10, y * 15);

                // ���������� ������� ������ �������
                if (roomPosition == Vector2.zero)
                {
                    continue;
                }

                GenerateRoom(roomPosition);
            }
        }
    }

    private void GenerateRoomsAroundPlayer()
    {
        Vector2 playerPosition = new Vector2(Mathf.Floor(player.position.x / 10) * 10, Mathf.Floor(player.position.z / 10) * 10);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 roomPosition = playerPosition + new Vector2(x * 10, y * 10);

                if (!generatedRooms.ContainsKey(roomPosition))
                {
                    GenerateRoom(roomPosition);
                }
            }
        }
    }

    private void GenerateRoom(Vector2 position)
    {
        float playerSanity = sanitySystem.currentSanity;

        // �������� ����� ��������� ��������� ��� ������ �������
        bool isNightmareRoom = false;

        if (playerSanity <= 50f && nightmareRoomPrefabs.Length > 0) // �������� ���� 50
        {
            float nightmareChanceModifier = nightmareRoomWeight;
            float randomChance = Random.value;

            if (randomChance < nightmareChanceModifier)
            {
                isNightmareRoom = true;
            }
        }

        // ���� ��������� �������
        if (isNightmareRoom)
        {
            GenerateSpecialRoom(nightmareRoomPrefabs, position);
            return;
        }

        // ���� ����������� ��� ������ �������
        GenerateSpecialRoom(roomPrefabs, position);
    }

    private void GenerateSpecialRoom(GameObject[] prefabArray, Vector2 position)
    {
        // ����� ������ ��������� ������� �� �������
        float totalWeight = 0f;
        foreach (float weight in prefabWeights)
        {
            totalWeight += weight;
        }

        float randomWeight = Random.Range(0f, totalWeight);
        float accumulatedWeight = 0f;

        for (int i = 0; i < prefabArray.Length; i++)
        {
            accumulatedWeight += prefabWeights[i];
            if (randomWeight <= accumulatedWeight)
            {
                GameObject roomPrefab = prefabArray[i];
                GameObject room = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
                generatedRooms[position] = room;

                // ��������� ������ � ����������� �������� � �������
                GenerateInteriorObjects(room);
                GenerateSpecialObjects(room);

                // ������������� NavMesh
                BuildRoomNavMesh(room);

                // ��������� ����� �������� � �������
                MonsterSpawner spawner = room.GetComponent<MonsterSpawner>();
                if (spawner != null)
                {
                    spawner.SpawnMonsters();
                }

                return;
            }
        }
    }

    private void GenerateInteriorObjects(GameObject room)
    {
        if (interiorPrefabs.Length == 0 || interiorWeights.Length != interiorPrefabs.Length)
        {
            Debug.LogError("����������� ��������� ������� ��� ���� ��� ������.");
            return;
        }

        // �������� ������� �������
        Collider roomCollider = room.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogError("������� �� ����� ���������� Collider.");
            return;
        }

        Bounds roomBounds = roomCollider.bounds;

        int numberOfObjects = Random.Range(1, 5); // ���������� �������� ������

        for (int i = 0; i < numberOfObjects; i++)
        {
            float totalWeight = 0f;
            foreach (float weight in interiorWeights)
            {
                totalWeight += weight;
            }

            float randomWeight = Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            GameObject selectedPrefab = null;
            for (int j = 0; j < interiorPrefabs.Length; j++)
            {
                accumulatedWeight += interiorWeights[j];
                if (randomWeight <= accumulatedWeight)
                {
                    selectedPrefab = interiorPrefabs[j];
                    break;
                }
            }

            if (selectedPrefab == null)
            {
                Debug.LogWarning("�� ������� ������� ������ ������.");
                continue;
            }

            Collider furnitureCollider = selectedPrefab.GetComponent<Collider>();
            if (furnitureCollider == null)
            {
                Debug.LogWarning($"������ {selectedPrefab.name} �� ����� ���������� Collider.");
                continue;
            }

            Bounds furnitureBounds = furnitureCollider.bounds;

            float xPos = Random.Range(roomBounds.min.x + wallThickness + furnitureBounds.size.x / 2, roomBounds.max.x - wallThickness - furnitureBounds.size.x / 2);
            float zPos = Random.Range(roomBounds.min.z + wallThickness + furnitureBounds.size.z / 2, roomBounds.max.z - wallThickness - furnitureBounds.size.z / 2);

            Vector3 spawnPosition = new Vector3(xPos, roomBounds.min.y + furnitureBounds.extents.y, zPos);

            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, room.transform);
        }
    }

    private void GenerateSpecialObjects(GameObject room)
    {
        Collider roomCollider = room.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogError("������� �� ����� ���������� Collider.");
            return;
        }

        Bounds roomBounds = roomCollider.bounds;

        // ������
        if (chandelierPrefab != null && Random.value > 0.5f)
        {
            Vector3 chandelierPosition = new Vector3(
                Random.Range(roomBounds.min.x + wallThickness, roomBounds.max.x - wallThickness),
                roomBounds.max.y - 1f, // ��� ��������
                Random.Range(roomBounds.min.z + wallThickness, roomBounds.max.z - wallThickness)
            );
            Instantiate(chandelierPrefab, chandelierPosition, Quaternion.identity, room.transform);
        }

        // ����
        if (windowPrefab != null && Random.value > 0.5f)
        {
            Vector3 windowPosition = new Vector3(
                Random.Range(roomBounds.min.x + wallThickness, roomBounds.max.x - wallThickness),
                roomBounds.center.y,
                roomBounds.min.z + wallThickness
            );
            Instantiate(windowPrefab, windowPosition, Quaternion.identity, room.transform);
        }
    }

    private void SpawnMonstersInRoom(GameObject room)
    {
        int monsterCount = Random.Range(minMonsters, maxMonsters);
        for (int i = 0; i < monsterCount; i++)
        {
            Vector3 spawnPosition = room.transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, room.transform);
        }
    }

    private void BuildRoomNavMesh(GameObject room)
    {
        NavMeshSurface navMeshSurface = room.GetComponent<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    private void DeactivateRoomsFarFromPlayer()
    {
        foreach (var room in generatedRooms)
        {
            float distance = Vector3.Distance(player.position, room.Value.transform.position);
            if (distance > deactivationDistance)
            {
                room.Value.SetActive(false);
            }
            else
            {
                room.Value.SetActive(true);
            }
        }
    }
}