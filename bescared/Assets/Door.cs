using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Настройки двери")]
    [SerializeField] private float openAngle = 90f; // Угол открытия двери
    [SerializeField] private float openSpeed = 2f; // Скорость открытия/закрытия
    [SerializeField] private float interactionDistance = 2f; // Дистанция взаимодействия
    [SerializeField] private float autoCloseDistance = 5f; // Дистанция для автоматического закрытия
    [SerializeField] private bool isLocked = false; // Заперта ли дверь
    [SerializeField] private AudioClip openSound; // Звук открытия
    [SerializeField] private AudioClip closeSound; // Звук закрытия
    [SerializeField] private AudioClip lockedSound; // Звук запертой двери

    [Header("Состояние двери")]
    [SerializeField] private bool startLocked = false; // Начальное состояние двери
    [SerializeField] private bool startOpen = false; // Начальное состояние открытости

    private bool isOpen = false; // Открыта ли дверь
    private Quaternion closedRotation; // Поворот закрытой двери
    private Quaternion openRotation; // Поворот открытой двери
    private AudioSource audioSource; // Источник звука
    private Transform player; // Игрок

    void Start()
    {
        // Инициализация поворотов
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        // Добавляем компонент для звука
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D звук
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;

        // Находим игрока
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag.");
        }

        // Устанавливаем начальное состояние
        isLocked = startLocked;
        isOpen = startOpen;
        if (isOpen)
        {
            transform.rotation = openRotation;
        }
    }

    void Update()
    {
        // Проверяем дистанцию до игрока
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        
        // Проверка нажатия клавиши E
        if (distance <= interactionDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isLocked)
                {
                    PlaySound(lockedSound);
                    return;
                }

                ToggleDoor();
            }
        }

        // Автоматическое закрытие двери
        if (isOpen && !isLocked && distance > autoCloseDistance)
        {
            isOpen = false;
            PlaySound(closeSound);
        }

        // Плавное открытие/закрытие двери
        if (isOpen)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime * openSpeed);
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        PlaySound(isOpen ? openSound : closeSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Метод для отпирания двери
    public void Unlock()
    {
        isLocked = false;
    }

    // Метод для запирания двери
    public void Lock()
    {
        isLocked = true;
        isOpen = false; // Закрываем дверь при запирании
    }

    // Визуализация радиусов в редакторе
    void OnDrawGizmosSelected()
    {
        // Радиус взаимодействия
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Радиус автоматического закрытия
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoCloseDistance);
    }
}
