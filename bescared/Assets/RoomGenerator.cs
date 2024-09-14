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
        // ��������� ������ ������� � ��������� �������
        GenerateRoom(Vector2.zero);
    }

    private void GenerateRoomsAroundPlayer()
    {
        Vector2 playerPosition = GetGridPosition(player.position);

        // ���������� ������� ������� ������
        Vector2 roomPosition = playerPosition + new Vector2(0, 1); // ������ �� ��� Z

        // ���������, �������� �� ������� ��� ����� �������
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

                // ��������� �������� ������ �������
                GenerateInteriorObjects(room);

                // �������� ����� �������
                float roomLength = room.GetComponent<MeshCollider>().bounds.size.z; // �������� ����� ������� �� ��� Z

                // ����������� ���������� Z �� ����� ������� ��� ��������� �������
                position.z += roomLength; // ��������� ������� ��� ��������� �������
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
        // ���������� ���������� ��������, ������� ����� ���������� ������ �������
        int numberOfObjects = Random.Range(1, 5); // ��������� ���������� �������� �� 1 �� 5

        // �������� ������� �������
        MeshCollider roomCollider = room.GetComponent<MeshCollider>();
        if (roomCollider == null)
        {
            Debug.LogError("Room does not have a MeshCollider component.");
            return;
        }

        float roomWidth = roomCollider.bounds.size.x; // ������ ������� �� ��� X
        float roomDepth = roomCollider.bounds.size.z; // ������� ������� �� ��� Z

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
                    Vector3 randomPosition = room.transform.position + new Vector3(Random.Range(-roomWidth / 2f, roomWidth / 2f), interiorHeight / 2f, Random.Range(-roomDepth / 2f, roomDepth / 2f));

                    Instantiate(interiorPrefab, randomPosition, Quaternion.identity, room.transform); // ���������� ������������ ������
                    Debug.Log($"Spawned {interiorPrefab.name} at {randomPosition} inside room.");
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

    private Vector2 GetGridPosition(Vector3 position)
    {
        return new Vector2(Mathf.Floor(position.x / 10) * 10, Mathf.Floor(position.z / 10) * 30);
    }
}