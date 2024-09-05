using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; // Массив префабов комнат
    public Transform player; // Ссылка на игрока
    public float generationDistance = 20f; // Расстояние, на котором генерируются новые комнаты
    public float deactivationDistance = 30f; // Расстояние, на котором комнаты будут деактивироваться
    public int width = 5; // Ширина карты
    public int height = 5; // Высота карты

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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 roomPosition = new Vector2(x * 10, y * 20); // Изменить на нужное расстояние
                GenerateRoom(roomPosition);
            }
        }
    }

    private void GenerateRoomsAroundPlayer()
    {
        Vector2 playerPosition = new Vector2(Mathf.Floor(player.position.x / 10) * 10, Mathf.Floor(player.position.z / 10) * 10);

        for (int x = -1; x <= -1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 roomPosition = playerPosition + new Vector2(x, y * 10);

                if (!generatedRooms.ContainsKey(roomPosition))
                {
                    GenerateRoom(roomPosition);
                }
            }
        }
    }

    private void GenerateRoom(Vector2 position)
    {
        GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        GameObject room = Instantiate(roomPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
        generatedRooms[position] = room;
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
}