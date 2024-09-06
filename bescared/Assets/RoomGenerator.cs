using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; // ������ �������� ������
    public float[] prefabWeights; // ������ ����� ��� ������� �������
    public GameObject[] interiorPrefabs; // ������ �������� ��� ���������� ���������� ������ �������
    public float[] interiorWeights; // ������ ����� ��� �������� ����������
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
                Vector2 roomPosition = new Vector2(x * 10, y * 30); // �������� �� ������ ����������
                GenerateRoom(roomPosition);
            }
        }
    }

    private void GenerateRoomsAroundPlayer()
    {
        Vector2 playerPosition = new Vector2(Mathf.Floor(player.position.x / 10) * 10, Mathf.Floor(player.position.z / 10) * 30); // ���������� ����� �������� ������

        for (int x = -1; x <= -1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 roomPosition = playerPosition + new Vector2(0, y * 30);

                if (!generatedRooms.ContainsKey(roomPosition))
                {
                    GenerateRoom(roomPosition);
                }
            }
        }
    }

    private void GenerateRoom(Vector2 position)
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

                // ��������� �������� ������ �������
                GenerateInteriorObjects(room);

                return;
            }
        }
    }

    private void GenerateInteriorObjects(GameObject room)
    {
        // ���������� ���������� ��������, ������� ����� ���������� ������ �������
        int numberOfObjects = Random.Range(1, 5); // ��������� ���������� �������� �� 1 �� 5

        // �������� ������� �������
        MeshCollider roomCollider = room.GetComponent<MeshCollider>();
        if (roomCollider == null)
        {
            Debug.LogError("Room does not have a MeshCollider component.");
            return;
        }

        float roomWidth = roomCollider.bounds.size.x;
        float roomDepth = roomCollider.bounds.size.z;

        for (int i = 0; i < numberOfObjects; i++)
        {
            // ����� ���������� ������� ���������
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

                    // �������� ������ ������� ���������
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

                    // ���������� ��������� ������� ������ �������
                    Vector3 randomPosition = room.transform.position + new Vector3(Random.Range(-roomWidth / 2f, roomWidth / 2f), interiorHeight / 2 + 3, Random.Range(-roomDepth / 2f, roomDepth / 2f));

                    Instantiate(interiorPrefab, randomPosition, Quaternion.identity, room.transform); // ���������� ������������ ������
                    break;
                }
            }
        }
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