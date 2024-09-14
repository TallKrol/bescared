using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; // Массив префабов комнат
    public float[] prefabWeights; // Массив весов для каждого префаба
    public GameObject[] interiorPrefabs; // Массив префабов для случайного размещения внутри комнаты
    public float[] interiorWeights; // Массив весов для префабов интерьеров
    public Transform player; // Ссылка на игрока
    public float generationDistance = 20f; // Расстояние, на котором генерируются новые комнаты
    public float deactivationDistance = 30f; // Расстояние, на котором комнаты будут деактивироваться

    private Vector3 lastPlayerPosition;
    private Dictionary<Vector2, GameObject> generatedRooms = new Dictionary<Vector2, GameObject>();

    private void Start()
    {
        lastPlayerPosition = player.position;
        GenerateInitialRooms();
    }

    private void Update()
    {
        if (Vector3.Distance(player.position, lastPlayerPosition) > generationDistance)
        {
            lastPlayerPosition = player.position;
            GenerateRoomsAroundPlayer();
        }

        // Проверка и отключение комнат, которые далеко от игрока
        DeactivateRoomsFarFromPlayer();
    }

    private void GenerateInitialRooms()
    {
        // Генерация первой комнаты в начальной позиции
        GenerateRoom(Vector2.zero);
    }

    private void GenerateRoomsAroundPlayer()
    {
        Vector2 playerPosition = GetGridPosition(player.position);

        // Генерируем комнату впереди игрока
        Vector2 roomPosition = playerPosition + new Vector2(0, 1); // Вперед по оси Z

        // Проверяем, свободна ли позиция для новой комнаты
        if (IsPositionFree(roomPosition))
        {
            GenerateRoom(roomPosition);
        }
    }

    private void GenerateRoom(Vector3 position)
    {
        float totalWeight = 0f;
        foreach (float weight in prefabWeights)
        {
            totalWeight += weight;
        }

        float randomWeight = Random.Range(0f, totalWeight);
        float accumulatedWeight = 0f;

        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            accumulatedWeight += prefabWeights[i];
            if (randomWeight <= accumulatedWeight)
            {
                GameObject roomPrefab = roomPrefabs[i];
                GameObject room = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
                generatedRooms[position] = room;

                // Генерация объектов внутри комнаты
                GenerateInteriorObjects(room);

                // Получаем длину комнаты
                float roomLength = room.GetComponent<MeshCollider>().bounds.size.z; // Получаем длину комнаты по оси Z

                // Увеличиваем координату Z на длину комнаты для следующей комнаты
                position.z += roomLength; // Обновляем позицию для следующей комнаты
                break;
            }
        }
    }

    private bool IsPositionFree(Vector2 position)
    {
        return !generatedRooms.ContainsKey(position);
    }

    private void GenerateInteriorObjects(GameObject room)
    {
        // Определить количество объектов, которые нужно разместить внутри комнаты
        int numberOfObjects = Random.Range(1, 5); // Случайное количество объектов от 1 до 5

        // Получить размеры комнаты
        MeshCollider roomCollider = room.GetComponent<MeshCollider>();
        if (roomCollider == null)
        {
            Debug.LogError("Room does not have a MeshCollider component.");
            return;
        }

        float roomWidth = roomCollider.bounds.size.x; // Ширина комнаты по оси X
        float roomDepth = roomCollider.bounds.size.z; // Глубина комнаты по оси Z

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Выбор случайного префаба интерьера
            float totalWeight = 0f;
            foreach (float weight in interiorWeights)
            {
                totalWeight += weight;
            }

            float randomWeight = Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            for (int j = 0; j < interiorPrefabs.Length; j++)
            {
                accumulatedWeight += interiorWeights[j];
                if (randomWeight <= accumulatedWeight)
                {
                    GameObject interiorPrefab = interiorPrefabs[j];

                    // Получите высоту префаба интерьера
                    float interiorHeight = 0f;
                    Collider interiorCollider = interiorPrefab.GetComponent<Collider>();
                    if (interiorCollider != null)
                    {
                        interiorHeight = interiorCollider.bounds.size.y;
                    }
                    else
                    {
                        Debug.LogWarning($"Interior prefab {interiorPrefab.name} does not have a Collider component.");
                    }

                    // Определите случайную позицию внутри комнаты
                    Vector3 randomPosition = room.transform.position + new Vector3(Random.Range(-roomWidth / 2f, roomWidth / 2f), interiorHeight / 2f, Random.Range(-roomDepth / 2f, roomDepth / 2f));

                    Instantiate(interiorPrefab, randomPosition, Quaternion.identity, room.transform); // Установить родительский объект
                    Debug.Log($"Spawned {interiorPrefab.name} at {randomPosition} inside room.");
                    break;
                }
            }
        }
    }

    private void DeactivateRoomsFarFromPlayer()
    {
        // Перебираем все сгенерированные комнаты
        foreach (var room in generatedRooms)
        {
            float distance = Vector3.Distance(player.position, room.Value.transform.position);
            if (distance > deactivationDistance)
            {
                // Отключаем комнату, если она далеко от игрока
                room.Value.SetActive(false);
            }
            else
            {
                // Включаем комнату, если она близко к игроку
                room.Value.SetActive(true);
            }
        }
    }

    private Vector2 GetGridPosition(Vector3 position)
    {
        return new Vector2(Mathf.Floor(position.x / 10) * 10, Mathf.Floor(position.z / 10) * 30);
    }
}