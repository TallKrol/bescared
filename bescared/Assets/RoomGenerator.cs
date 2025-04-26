using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class RoomGenerator : MonoBehaviour
{
    [Header("Standard Rooms")]
    public GameObject firstRoomPrefab;
    public GameObject[] roomPrefabs;
    public float[] prefabWeights;

    [Header("Special Rooms")]
    public GameObject[] nightmareRoomPrefabs;
    public float nightmareRoomWeight = 0.2f;
    public GameObject[] peacefulRoomPrefabs;
    public float peacefulRoomWeight = 0.8f;

    [Header("Monsters")]
    public GameObject monsterPrefab;
    public int minMonsters = 5;
    public int maxMonsters = 15;

    [Header("Player and Sanity")]
    public Transform player;
    public SanitySystem sanitySystem;

    [Header("Generation Settings")]
    public float generationDistance = 20f;
    public float deactivationDistance = 30f;
    public int roomsAhead = 3; // Количество комнат впереди игрока

    private Vector3 lastPlayerPosition;
    private Dictionary<float, GameObject> generatedRooms = new Dictionary<float, GameObject>();
    private float lastRoomEndPosition = 0f;

    private void Start()
    {
        lastPlayerPosition = player.position;
        GenerateFirstRoom();
        GenerateInitialRooms();
    }

    private void GenerateFirstRoom()
    {
        if (firstRoomPrefab == null)
        {
            Debug.LogError("Префаб первой комнаты не назначен!");
            return;
        }

        float firstRoomPosition = 0f;
        GameObject firstRoom = InstantiateRoom(firstRoomPrefab, firstRoomPosition);
        if (firstRoom != null)
        {
            generatedRooms[firstRoomPosition] = firstRoom;
            lastRoomEndPosition = GetRoomLength(firstRoom);
        }
    }

    private void GenerateInitialRooms()
    {
        for (int i = 0; i < roomsAhead; i++)
        {
            float nextRoomPosition = lastRoomEndPosition;
            GenerateRoom(nextRoomPosition);
        }
    }

    private GameObject InstantiateRoom(GameObject prefab, float position)
    {
        Vector3 worldPosition = new Vector3(0, 0, position);
        GameObject room = Instantiate(prefab, worldPosition, Quaternion.identity);
        return room;
    }

    private float GetRoomLength(GameObject room)
    {
        Collider roomCollider = room.GetComponent<Collider>();
        if (roomCollider != null)
        {
            return roomCollider.bounds.size.z;
        }
        return 0f;
    }

    private void GenerateRoom(float position)
    {
        float playerSanity = sanitySystem.currentSanity;
        bool isNightmareRoom = false;

        if (playerSanity <= 50f && nightmareRoomPrefabs.Length > 0)
        {
            float randomChance = Random.value;
            if (randomChance < nightmareRoomWeight)
            {
                isNightmareRoom = true;
            }
        }

        GameObject roomPrefab = isNightmareRoom ? 
            GetRandomRoomPrefab(nightmareRoomPrefabs) : 
            GetRandomRoomPrefab(roomPrefabs);

        if (roomPrefab != null)
        {
            GameObject room = InstantiateRoom(roomPrefab, position);
            if (room != null)
            {
                generatedRooms[position] = room;
                lastRoomEndPosition = position + GetRoomLength(room);
                
                // Спавним монстров
                MonsterSpawner spawner = room.GetComponent<MonsterSpawner>();
                if (spawner != null)
                {
                    spawner.SpawnMonsters();
                }
                
                BuildRoomNavMesh(room);
            }
        }
    }

    private GameObject GetRandomRoomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return null;
        return prefabs[Random.Range(0, prefabs.Length)];
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

    private void GenerateRoomsAroundPlayer()
    {
        float playerZ = player.position.z;
        float lastRoomZ = lastRoomEndPosition;

        if (playerZ > lastRoomZ - generationDistance)
        {
            float nextRoomPosition = lastRoomZ;
            GenerateRoom(nextRoomPosition);
        }
    }

    private void DeactivateRoomsFarFromPlayer()
    {
        foreach (var room in generatedRooms)
        {
            float distance = Mathf.Abs(player.position.z - room.Key);
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

    private void BuildRoomNavMesh(GameObject room)
    {
        NavMeshSurface navMeshSurface = room.GetComponent<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }
}