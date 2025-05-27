using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Merchant : MonoBehaviour, IDamageable
{
    [Header("Merchant Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float aggressionThreshold = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Trading Settings")]
    [SerializeField] private List<MerchantItem> availableItems = new List<MerchantItem>();
    [SerializeField] private int minItemsToSpawn = 3;
    [SerializeField] private int maxItemsToSpawn = 6;
    [SerializeField] private Transform[] itemSpawnPoints;

    [Header("Components")]
    [SerializeField] private GameObject counter;
    [SerializeField] private GameObject bell;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bellSound;
    [SerializeField] private AudioClip angrySound;
    [SerializeField] private AudioClip tradeSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private Animator animator;

    // Private fields
    private float lastAttackTime;
    private Transform player;
    private bool isChasing = false;
    private bool isAggressive = false;
    private List<MerchantItem> currentItems = new List<MerchantItem>();
    private Dictionary<MerchantItem, GameObject> spawnedItems = new Dictionary<MerchantItem, GameObject>();
    private bool isInitialized = false;

    private void Start()
    {
        InitializeMerchant();
    }

    private void InitializeMerchant()
    {
        if (isInitialized) return;

        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.player_not_found"));
        }

        ValidateComponents();
        SpawnRandomItems();
        isInitialized = true;
    }

    private void ValidateComponents()
    {
        if (counter == null) Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.component_missing", "Counter"));
        if (bell == null) Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.component_missing", "Bell"));
        if (audioSource == null) Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.component_missing", "AudioSource"));
        if (animator == null) Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.component_missing", "Animator"));
        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0)
        {
            Debug.LogError(LocalizationManager.Instance.GetTranslation("merchant.no_spawn_points"));
        }
    }

    private void Update()
    {
        if (!isInitialized || player == null) return;

        if (isAggressive)
        {
            HandleAggressiveBehavior();
        }
        else
        {
            HandleNormalBehavior();
        }
    }

    private void HandleAggressiveBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            TryAttack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
    }

    private void HandleNormalBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            // Можно добавить логику для нормального поведения
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        PlaySound(attackSound);

        // Наносим урон игроку
        IDamageable playerDamageable = player.GetComponent<IDamageable>();
        if (playerDamageable != null)
        {
            playerDamageable.TakeDamage(attackDamage);
        }
    }

    private void ChasePlayer()
    {
        if (isChasing)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        PlaySound(angrySound);

        if (currentHealth <= maxHealth * aggressionThreshold)
        {
            BecomeAggressive();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void BecomeAggressive()
    {
        if (!isAggressive)
        {
            isAggressive = true;
            PlaySound(angrySound);
            Debug.Log(LocalizationManager.Instance.GetTranslation("merchant.aggressive"));
            if (animator != null)
            {
                animator.SetBool("IsAggressive", true);
            }
        }
    }

    private void Die()
    {
        // Логика смерти торговца
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        Destroy(gameObject, 2f);
    }

    private void SpawnRandomItems()
    {
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning(LocalizationManager.Instance.GetTranslation("merchant.no_items"));
            return;
        }

        if (itemSpawnPoints == null || itemSpawnPoints.Length == 0)
        {
            Debug.LogError(LocalizationManager.Instance.GetTranslation("merchant.no_spawn_points"));
            return;
        }

        int itemsToSpawn = Random.Range(minItemsToSpawn, maxItemsToSpawn + 1);
        currentItems = availableItems.OrderBy(x => Random.value).Take(itemsToSpawn).ToList();

        foreach (var item in currentItems)
        {
            if (item == null) continue;

            Transform spawnPoint = itemSpawnPoints[Random.Range(0, itemSpawnPoints.Length)];
            GameObject itemObject = Instantiate(item.gameObject, spawnPoint.position, spawnPoint.rotation);
            spawnedItems[item] = itemObject;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void OnItemPurchased(MerchantItem item)
    {
        if (item == null) return;

        PlaySound(tradeSound);
        Debug.Log(LocalizationManager.Instance.GetTranslation("merchant.trade"));
        if (spawnedItems.ContainsKey(item))
        {
            Destroy(spawnedItems[item]);
            spawnedItems.Remove(item);
            currentItems.Remove(item);
        }
    }

    public void RingBell()
    {
        if (bell != null)
        {
            PlaySound(bellSound);
            // Можно добавить анимацию звонка
        }
    }
} 