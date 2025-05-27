using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupRange = 2f;
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 2f;

    private Vector3 startPosition;
    private bool isHovering = true;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isHovering)
        {
            // Плавное движение вверх-вниз
            float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Проверяем расстояние до игрока
        if (PlayerInventory.Instance != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerInventory.Instance.transform.position);
            if (distanceToPlayer <= pickupRange)
            {
                // Показываем подсказку о подборе
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Pickup();
                }
            }
        }
    }

    private void Pickup()
    {
        if (PlayerInventory.Instance.AddItem(item))
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса подбора в редакторе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
} 