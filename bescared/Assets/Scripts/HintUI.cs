using UnityEngine;
using TMPro;
using System.Collections;

public class HintUI : MonoBehaviour
{
    public static HintUI Instance { get; private set; }

    [Header("Элементы интерфейса")]
    public TextMeshProUGUI hintText;
    public float displayTime = 3f;
    public float fadeSpeed = 1f;

    private Coroutine currentHintCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Скрываем подсказку при старте
        hintText.alpha = 0f;
    }

    public void ShowHint(string message)
    {
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
        }
        currentHintCoroutine = StartCoroutine(ShowHintCoroutine(message));
    }

    private IEnumerator ShowHintCoroutine(string message)
    {
        // Показываем подсказку
        hintText.text = message;
        hintText.alpha = 1f;

        // Ждем указанное время
        yield return new WaitForSeconds(displayTime);

        // Плавно скрываем подсказку
        while (hintText.alpha > 0f)
        {
            hintText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        hintText.alpha = 0f;
        currentHintCoroutine = null;
    }
} 