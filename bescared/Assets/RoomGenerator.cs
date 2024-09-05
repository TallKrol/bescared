using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; // ������ �������� ������
    public Transform player; // ������ �� ������
    public float generationDistance = 20f; // ����������, �� ������� ������������ ����� �������
    public float deactivationDistance = 30f; // ����������, �� ������� ������� ����� ����������������
    public int width = 5; // ������ �����
    public int height = 5; // ������ �����

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

        // �������� � ���������� ������, ������� ������ �� ������
        DeactivateRoomsFarFromPlayer();
    }

    private void GenerateInitialRooms()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 roomPosition = new Vector2(x * 10, y * 20); // �������� �� ������ ����������
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
        // ���������� ��� ��������������� �������
        foreach (var room in generatedRooms)
        {
            float distance = Vector3.Distance(player.position, room.Value.transform.position);
            if (distance > deactivationDistance)
            {
                // ��������� �������, ���� ��� ������ �� ������
                room.Value.SetActive(false);
            }
            else
            {
                // �������� �������, ���� ��� ������ � ������
                room.Value.SetActive(true);
            }
        }
    }
}