using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class RoomGenerator : MonoBehaviour
{
    [Header("Standard Rooms")]
    public GameObject firstRoomPrefab; // Префаб для первой комнаты
    public GameObject[] roomPrefabs; // Массив обычных префабов комнат
    public float[] prefabWeights; // Массив весов для выбора обычных комнат

    [Header("Special Rooms")]
    public GameObject[] nightmareRoomPrefabs; // Массив префабов кошмарных комнат
    public float nightmareRoomWeight = 0.2f; // Коэффициент шанса для кошмарных комнат
    public GameObject[] peacefulRoomPrefabs; // Массив префабов мирных комнат
    public float peacefulRoomWeight = 0.8f; // Коэффициент шанса для мирных комнат

    [Header("Interior")]
    public GameObject[] interiorPrefabs; // Массив префабов для мебели
    public float[] interiorWeights; // Массив весов для выбора мебели
    public GameObject chandelierPrefab; // Префаб для люстры
    public GameObject windowPrefab; // Префаб для окна
    public GameObject picturePrefab; // Префаб для картины

    [Header("Monsters")]
    public GameObject monsterPrefab; // Префаб монстра
    public int minMonsters = 5; // Минимальное количество монстров
    public int maxMonsters = 15; // Максимальное количество монстров

    [Header("Player and Sanity")]
    public Transform player; // Ссылка на объект игрока
    public SanitySystem sanitySystem; // Система рассудка игрока

    [Header("Generation Settings")]
    public float generationDistance = 20f; // Расстояние для генерации новых комнат
    public float deactivationDistance = 30f; // Расстояние для деактивации комнат
    public int width = 5; // Ширина начальной сетки комнат
    public int height = 5; // Высота начальной сетки комнат
    public float wallThickness = 1f; // Толщина стен

    private Vector3 lastPlayerPosition;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>();

    private void Start()
    {
        lastPlayerPosition = player.position;

        // Генерация первой комнаты
        GenerateFirstRoom();

        // Генерация остальных начальных комнат
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
            Debug.LogError("Префаб первой комнаты не назначен!");
            return;
        }

        // Создаём первую комнату в фиксированной позиции
        Vector2 firstRoomPosition = Vector2.zero;
        GameObject firstRoom = Instantiate(firstRoomPrefab, new Vector3(firstRoomPosition.x, 0, firstRoomPosition.y), Quaternion.identity);
        generatedRooms[firstRoomPosition] = firstRoom;

        // Генерация мебели в первой комнате
        GenerateInteriorObjects(firstRoom);
        GenerateSpecialObjects(firstRoom);

        // Перестраиваем NavMesh для первой комнаты
        BuildRoomNavMesh(firstRoom);
    }

    private void GenerateInitialRooms()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 roomPosition = new Vector2(x * 10, y * 15);

                // Пропускаем позицию первой комнаты
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

        // Проверка шанса генерации кошмарной или мирной комнаты
        bool isNightmareRoom = false;

        if (playerSanity <= 50f && nightmareRoomPrefabs.Length > 0) // Рассудок ниже 50
        {
            float nightmareChanceModifier = nightmareRoomWeight;
            float randomChance = Random.value;

            if (randomChance < nightmareChanceModifier)
            {
                isNightmareRoom = true;
            }
        }

        // Если кошмарная комната
        if (isNightmareRoom)
        {
            GenerateSpecialRoom(nightmareRoomPrefabs, position);
            return;
        }

        // Если стандартная или мирная комната
        GenerateSpecialRoom(roomPrefabs, position);
    }

    private void GenerateSpecialRoom(GameObject[] prefabArray, Vector2 position)
    {
        // Общая логика генерации комнаты из массива
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

                // Генерация мебели и специальных объектов в комнате
                GenerateInteriorObjects(room);
                GenerateSpecialObjects(room);

                // Перестраиваем NavMesh
                BuildRoomNavMesh(room);

                // Локальный спавн монстров в комнате
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
            Debug.LogError("Неправильно настроены префабы или веса для мебели.");
            return;
        }

        // Получаем размеры комнаты
        Collider roomCollider = room.GetComponent<Collider>();
        if (roomCollider == null)
        {
            Debug.LogError("Комната не имеет компонента Collider.");
            return;
        }

        Bounds roomBounds = roomCollider.bounds;

        int numberOfObjects = Random.Range(1, 5); // Количество объектов мебели

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
                Debug.LogWarning("Не удалось выбрать префаб мебели.");
                continue;
            }

            Collider furnitureCollider = selectedPrefab.GetComponent<Collider>();
            if (furnitureCollider == null)
            {
                Debug.LogWarning($"Префаб {selectedPrefab.name} не имеет компонента Collider.");
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
            Debug.LogError("Комната не имеет компонента Collider.");
            return;
        }

        Bounds roomBounds = roomCollider.bounds;

        // Люстра
        if (chandelierPrefab != null && Random.value > 0.5f)
        {
            Vector3 chandelierPosition = new Vector3(
                Random.Range(roomBounds.min.x + wallThickness, roomBounds.max.x - wallThickness),
                roomBounds.max.y - 1f, // Под потолком
                Random.Range(roomBounds.min.z + wallThickness, roomBounds.max.z - wallThickness)
            );
            Instantiate(chandelierPrefab, chandelierPosition, Quaternion.identity, room.transform);
        }

        // Окно
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