using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifetime = 2f;
    public int damage = 20;
    public GameObject hitEffect;
    public GameObject trail;

    private Vector3 direction;
    private float distance;
    private float traveledDistance = 0f;
    private bool hasHit = false;
    private Material bulletMaterial;
    private Light bulletLight;
    private float startTime;

    void Start()
    {
        // Создаем материал для пули
        bulletMaterial = new Material(Shader.Find("Standard"));
        bulletMaterial.color = Color.yellow;
        bulletMaterial.EnableKeyword("_EMISSION");
        bulletMaterial.SetColor("_EmissionColor", Color.yellow);
        GetComponent<MeshRenderer>().material = bulletMaterial;

        // Добавляем свет
        bulletLight = gameObject.AddComponent<Light>();
        bulletLight.color = Color.yellow;
        bulletLight.intensity = 2f;
        bulletLight.range = 2f;

        // Создаем трейл, если он задан
        if (trail != null)
        {
            GameObject trailInstance = Instantiate(trail, transform.position, Quaternion.identity);
            trailInstance.transform.SetParent(transform);
        }

        startTime = Time.time;
    }

    public void Initialize(Vector3 startPosition, Vector3 direction, float range)
    {
        transform.position = startPosition;
        this.direction = direction.normalized;
        this.distance = range;
        traveledDistance = 0f;
        hasHit = false;
    }

    void Update()
    {
        if (hasHit) return;

        float moveDistance = speed * Time.deltaTime;
        Vector3 newPosition = transform.position + direction * moveDistance;
        traveledDistance += moveDistance;

        // Проверяем столкновение с помощью Raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, moveDistance))
        {
            HandleHit(hit);
            return;
        }

        transform.position = newPosition;

        // Уничтожаем пулю, если она достигла максимальной дистанции или времени жизни
        if (traveledDistance >= distance || Time.time - startTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        hasHit = true;

        // Создаем эффект попадания
        if (hitEffect != null)
        {
            Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // Наносим урон
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // Уничтожаем пулю
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Очищаем материал
        if (bulletMaterial != null)
        {
            Destroy(bulletMaterial);
        }
    }
} 