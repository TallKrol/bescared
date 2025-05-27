using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    public enum LockType
    {
        None,
        Key,
        Code,
        Blood
    }

    [Header("Настройки двери")]
    public LockType lockType = LockType.None;
    public string doorId; // Уникальный идентификатор двери для сохранения
    public float openAngle = 90f;
    public float closeAngle = 0f;
    public float smooth = 2f;
    public float interactionDistance = 2f;
    public float autoCloseDistance = 5f;

    [Header("Настройки замка")]
    public string requiredKeyId; // ID ключа для двери с ключом
    public string doorCode; // Код для двери с кодовым замком
    public bool isLocked = true;
    public bool isOpen = false;

    [Header("Настройки крови")]
    [Tooltip("Требуется ли кровь для открытия двери")]
    public bool requiresBlood = false;

    [Header("Звуковые эффекты")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    public AudioClip unlockSound;
    private AudioSource audioSource;

    [Header("Визуальные эффекты")]
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public Material bloodyMaterial; // Материал для кровавой двери
    public Renderer doorRenderer;
    public bool isBloody = false; // Флаг кровавой двери

    private Transform player;
    private bool isInteracting = false;
    private bool isAutoClosing = false;
    private Coroutine autoCloseCoroutine;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Загружаем состояние двери
        LoadDoorState();
    }

    private void Update()
    {
        if (isOpen && !isInteracting)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > autoCloseDistance && !isAutoClosing)
            {
                StartAutoClose();
            }
        }

        if (isInteracting)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > interactionDistance)
            {
                StopInteraction();
            }
        }
    }

    public void Interact()
    {
        if (isInteracting) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactionDistance)
        {
            isInteracting = true;

            if (isLocked)
            {
                switch (lockType)
                {
                    case LockType.Key:
                        TryOpenWithKey();
                        break;
                    case LockType.Code:
                        OpenCodeInput();
                        break;
                    case LockType.Blood:
                        CheckBloodCondition();
                        break;
                }
            }
            else
            {
                ToggleDoor();
            }
        }
    }

    private void TryOpenWithKey()
    {
        // Проверяем наличие ключа у игрока
        if (PlayerInventory.HasKey(requiredKeyId))
        {
            UnlockDoor();
        }
        else
        {
            PlaySound(lockedSound);
            // Показываем подсказку о необходимости ключа
            ShowHint("Требуется ключ: " + requiredKeyId);
        }
    }

    private void OpenCodeInput()
    {
        // Открываем интерфейс ввода кода
        CodeInputUI.Instance.ShowCodeInput(this);
    }

    private void CheckBloodCondition()
    {
        if (PlayerInventory.HasBlood())
        {
            // Используем одну бутылку крови
            PlayerInventory.Instance.UseBlood();
            UnlockDoor();
            isBloody = true;
            UpdateDoorMaterial();
        }
        else
        {
            PlaySound(lockedSound);
            ShowHint("Требуется кровь. Убейте монстров в комнате.");
        }
    }

    public void UnlockDoor()
    {
        isLocked = false;
        PlaySound(unlockSound);
        UpdateDoorMaterial();
        SaveDoorState();
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        PlaySound(isOpen ? openSound : closeSound);
        SaveDoorState();
    }

    private void StartAutoClose()
    {
        isAutoClosing = true;
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(1f);
        isOpen = false;
        PlaySound(closeSound);
        isAutoClosing = false;
        SaveDoorState();
    }

    private void StopInteraction()
    {
        isInteracting = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void UpdateDoorMaterial()
    {
        if (doorRenderer != null)
        {
            if (isBloody)
            {
                doorRenderer.material = bloodyMaterial;
            }
            else
            {
                doorRenderer.material = isLocked ? lockedMaterial : unlockedMaterial;
            }
        }
    }

    private void ShowHint(string message)
    {
        // Показываем подсказку игроку
        HintUI.Instance.ShowHint(message);
    }

    private void SaveDoorState()
    {
        // Сохраняем состояние двери
        PlayerPrefs.SetInt("Door_" + doorId + "_Locked", isLocked ? 1 : 0);
        PlayerPrefs.SetInt("Door_" + doorId + "_Open", isOpen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadDoorState()
    {
        // Загружаем состояние двери
        if (PlayerPrefs.HasKey("Door_" + doorId + "_Locked"))
        {
            isLocked = PlayerPrefs.GetInt("Door_" + doorId + "_Locked") == 1;
        }
        if (PlayerPrefs.HasKey("Door_" + doorId + "_Open"))
        {
            isOpen = PlayerPrefs.GetInt("Door_" + doorId + "_Open") == 1;
        }
        UpdateDoorMaterial();
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
