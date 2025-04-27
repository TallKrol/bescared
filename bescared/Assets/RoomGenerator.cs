using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

[System.Serializable]
public class RoomData
{
    public GameObject prefab;
    public float weight;
    public float length;
}

public class RoomGenerator : MonoBehaviour
{
    [Header("Standard Rooms")]
    public RoomData firstRoomData;
    public RoomData[] roomData;

    [Header("Special Rooms")]
    public RoomData[] nightmareRoomData;
    public float nightmareRoomWeight = 0.2f;
    public RoomData[] peacefulRoomData;
    public float peacefulRoomWeight = 0.8f;

    [Header("Monsters")]
    public GameObject monsterPrefab;
    public int minMonsters = 5;
    public int maxMonsters = 15;

    [Header("Player and Sanity")]
    public Transform player;
    public SanitySystem sanitySystem;

    [Header("Generation Settings")]
    public float generationDistance = 40f; // Расстояние до конца последней комнаты, при котором нужно генерировать новую
    public float deactivationDistance = 60f;

    private Dictionary<float, GameObject> generatedRooms = new Dictionary<float, GameObject>();
    private float lastRoomEndPosition = 0f;

    private void Start()
    {
        if (player == null || sanitySystem == null) return;
        GenerateFirstRoom();
    }

    private void GenerateFirstRoom()
    {
        if (firstRoomData == null || firstRoomData.prefab == null) return;

        float firstRoomPosition = 0f;
        GameObject firstRoom = InstantiateRoom(firstRoomData.prefab, firstRoomPosition);
        if (firstRoom != null)
        {
            generatedRooms[firstRoomPosition] = firstRoom;
            lastRoomEndPosition = firstRoomPosition + firstRoomData.length;
        }
    }

    private GameObject InstantiateRoom(GameObject prefab, float position)
    {
        Vector3 worldPosition = new Vector3(0, 0, position);
        GameObject room = Instantiate(prefab, worldPosition, Quaternion.identity);
        return room;
    }

    private void GenerateRoom(float position)
    {
        float playerSanity = sanitySystem.currentSanity;
        bool isNightmareRoom = false;

        if (playerSanity <= 50f && nightmareRoomData != null && nightmareRoomData.Length > 0)
        {
            float randomChance = Random.value;
            if (randomChance < nightmareRoomWeight)
            {
                isNightmareRoom = true;
            }
        }

        RoomData selectedRoomData = isNightmareRoom ? 
            GetRandomRoomData(nightmareRoomData) : 
            GetRandomRoomData(roomData);

        if (selectedRoomData != null && selectedRoomData.prefab != null)
        {
            GameObject room = InstantiateRoom(selectedRoomData.prefab, position);
            
            if (room != null)
            {
                generatedRooms[position] = room;
                lastRoomEndPosition = position + selectedRoomData.length;
                
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

    private RoomData GetRandomRoomData(RoomData[] roomDataArray)
    {
        if (roomDataArray == null || roomDataArray.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var data in roomDataArray)
        {
            if (data != null)
            {
                totalWeight += data.weight;
            }
        }

        if (totalWeight <= 0f) return roomDataArray[0];

        float randomWeight = Random.Range(0f, totalWeight);
        float accumulatedWeight = 0f;

        foreach (var data in roomDataArray)
        {
            if (data != null)
            {
                accumulatedWeight += data.weight;
                if (randomWeight <= accumulatedWeight)
                {
                    return data;
                }
            }
        }

        return roomDataArray[0];
    }

    private void Update()
    {
        if (player == null) return;

        float playerZ = player.position.z;
        float distanceToLastRoom = lastRoomEndPosition - playerZ;

        if (distanceToLastRoom < generationDistance)
        {
            float nextRoomPosition = lastRoomEndPosition;
            GenerateRoom(nextRoomPosition);
        }

        DeactivateRoomsFarFromPlayer();
    }

    private void DeactivateRoomsFarFromPlayer()
    {
        foreach (var room in generatedRooms)
        {
            float distance = Mathf.Abs(player.position.z - room.Key);
            room.Value.SetActive(distance <= deactivationDistance);
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