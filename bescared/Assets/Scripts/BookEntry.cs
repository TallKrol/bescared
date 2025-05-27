using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Entry", menuName = "Monster Book/Monster Entry")]
public class BookEntry : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName;
    public Sprite monsterImage;
    [TextArea(3, 5)]
    public string description;

    [Header("Study Info")]
    [TextArea(3, 5)]
    public string behavior;
    [TextArea(3, 5)]
    public string countermeasures;
    [TextArea(3, 5)]
    public string gameplayNotes;

    [Header("Runtime Data")]
    [SerializeField] private bool isDiscovered;
    [SerializeField] private bool isStudied;
    [SerializeField] private int deathCount;
    [SerializeField] private float discoveryTime;

    public bool IsDiscovered => isDiscovered;
    public bool IsStudied => isStudied;
    public int DeathCount => deathCount;
    public float DiscoveryTime => discoveryTime;

    public void Discover()
    {
        if (!isDiscovered)
        {
            isDiscovered = true;
            discoveryTime = Time.time;
        }
    }

    public void Study()
    {
        isStudied = true;
        deathCount++;
    }

    public void Reset()
    {
        isDiscovered = false;
        isStudied = false;
        deathCount = 0;
        discoveryTime = 0f;
    }
} 