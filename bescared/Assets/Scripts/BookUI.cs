using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class BookUI : MonoBehaviour
{
    public static BookUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject bookPanel;
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI pageTitle;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TextMeshProUGUI pageNumberText;

    [Header("Page Settings")]
    [SerializeField] private int entriesPerPage = 2;
    [SerializeField] private float pageTurnDuration = 0.5f;
    [SerializeField] private AnimationCurve pageTurnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Entry UI Elements")]
    [SerializeField] private TextMeshProUGUI monsterNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI behaviorText;
    [SerializeField] private TextMeshProUGUI countermeasuresText;
    [SerializeField] private TextMeshProUGUI gameplayNotesText;
    [SerializeField] private TextMeshProUGUI deathCountText;
    [SerializeField] private Image monsterImage;

    private List<BookEntry> allEntries = new List<BookEntry>();
    private int currentPage = 0;
    private int totalPages = 0;
    private bool isTurningPage = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBook);
        }
        else
        {
            Debug.LogWarning("Close button not assigned in BookUI!");
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(PreviousPage);
        }
        else
        {
            Debug.LogWarning("Previous page button not assigned in BookUI!");
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(NextPage);
        }
        else
        {
            Debug.LogWarning("Next page button not assigned in BookUI!");
        }

        ValidateRequiredComponents();
    }

    private void ValidateRequiredComponents()
    {
        if (bookPanel == null) Debug.LogError("Book panel not assigned in BookUI!");
        if (entriesContainer == null) Debug.LogError("Entries container not assigned in BookUI!");
        if (entryPrefab == null) Debug.LogError("Entry prefab not assigned in BookUI!");
        if (entryPrefab != null && entryPrefab.GetComponent<BookEntryUI>() == null)
        {
            Debug.LogError("Entry prefab does not contain BookEntryUI component!");
        }
    }

    public void OpenBook()
    {
        if (bookPanel == null)
        {
            Debug.LogError("Cannot open book: book panel is not assigned!");
            return;
        }

        bookPanel.SetActive(true);
        RefreshEntries();
        UpdatePageButtons();
    }

    public void CloseBook()
    {
        if (bookPanel == null)
        {
            Debug.LogError("Cannot close book: book panel is not assigned!");
            return;
        }

        bookPanel.SetActive(false);
    }

    private void RefreshEntries()
    {
        if (entriesContainer == null || entryPrefab == null)
        {
            Debug.LogError("Cannot refresh entries: required components are missing!");
            return;
        }

        if (MonsterBook.Instance == null)
        {
            Debug.LogError("Cannot refresh entries: MonsterBook instance not found!");
            return;
        }

        // Очищаем контейнер
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }

        // Получаем все записи в порядке обнаружения
        allEntries = MonsterBook.Instance.GetEntriesInDiscoveryOrder();
        totalPages = Mathf.CeilToInt(allEntries.Count / (float)entriesPerPage);

        // Показываем записи для текущей страницы
        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (entriesContainer == null || entryPrefab == null) return;

        int startIndex = currentPage * entriesPerPage;
        int endIndex = Mathf.Min(startIndex + entriesPerPage, allEntries.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var entryUI = Instantiate(entryPrefab, entriesContainer);
            var entryComponent = entryUI.GetComponent<BookEntryUI>();
            if (entryComponent != null)
            {
                entryComponent.SetEntry(allEntries[i]);
            }
            else
            {
                Debug.LogError($"Entry UI component not found on instantiated prefab at index {i}!");
                Destroy(entryUI);
            }
        }

        UpdatePageNumber();
    }

    private void UpdatePageNumber()
    {
        if (pageNumberText != null)
        {
            pageNumberText.text = LocalizationManager.Instance.GetTranslation("book.page", currentPage + 1, totalPages);
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 0 && !isTurningPage)
        {
            StartPageTurn(-1);
        }
    }

    private void NextPage()
    {
        if (currentPage < totalPages - 1 && !isTurningPage)
        {
            StartPageTurn(1);
        }
    }

    private IEnumerator PageTurnCoroutine(int direction)
    {
        isTurningPage = true;
        float elapsed = 0f;
        float startRotation = direction > 0 ? 0 : 180;
        float endRotation = direction > 0 ? 180 : 0;
        float currentRotation = startRotation;

        while (elapsed < pageTurnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pageTurnDuration);
            float curveT = pageTurnCurve.Evaluate(t);
            currentRotation = Mathf.Lerp(startRotation, endRotation, curveT);
            yield return null;
        }

        // Очищаем и показываем новую страницу
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
        ShowCurrentPage();
        UpdatePageButtons();
        isTurningPage = false;
    }

    private void StartPageTurn(int direction)
    {
        currentPage += direction;
        StartCoroutine(PageTurnCoroutine(direction));
    }

    private void UpdatePageButtons()
    {
        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPage < totalPages - 1;
        }
    }

    private void Update()
    {
        // Открытие/закрытие книги по клавише B
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (bookPanel != null && !bookPanel.activeSelf)
            {
                OpenBook();
            }
            else
            {
                CloseBook();
            }
        }
    }
}

// Компонент для отдельной записи в книге
public class BookEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI behaviorText;
    [SerializeField] private TextMeshProUGUI countermeasuresText;
    [SerializeField] private TextMeshProUGUI gameplayNotesText;
    [SerializeField] private TextMeshProUGUI deathCountText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private Button entryButton;

    [Header("Visual Effects")]
    [SerializeField] private GameObject discoveredEffect;
    [SerializeField] private GameObject studiedEffect;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.2f;

    private BookEntry currentEntry;
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
        if (entryButton != null)
        {
            entryButton.onClick.AddListener(OnEntryClick);
        }
        else
        {
            Debug.LogWarning("Entry button not assigned in BookEntryUI!");
        }
    }

    public void SetEntry(BookEntry entry)
    {
        if (entry == null)
        {
            Debug.LogError("Cannot set null entry in BookEntryUI!");
            return;
        }

        currentEntry = entry;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentEntry == null) return;

        // Основная информация
        if (nameText != null) nameText.text = currentEntry.monsterName;
        if (descriptionText != null) descriptionText.text = currentEntry.description;
        if (monsterImage != null) monsterImage.sprite = currentEntry.monsterImage;
        if (deathCountText != null) deathCountText.text = LocalizationManager.Instance.GetTranslation("book.deaths", currentEntry.DeathCount);

        // Эффекты обнаружения/изучения
        if (discoveredEffect != null) discoveredEffect.SetActive(currentEntry.IsDiscovered);
        if (studiedEffect != null) studiedEffect.SetActive(currentEntry.IsStudied);

        // Дополнительная информация
        if (currentEntry.IsStudied)
        {
            if (behaviorText != null) behaviorText.text = currentEntry.behavior;
            if (countermeasuresText != null) countermeasuresText.text = currentEntry.countermeasures;
            if (gameplayNotesText != null) gameplayNotesText.text = currentEntry.gameplayNotes;
        }
        else
        {
            string unknown = LocalizationManager.Instance.GetTranslation("book.unknown");
            if (behaviorText != null) behaviorText.text = unknown;
            if (countermeasuresText != null) countermeasuresText.text = unknown;
            if (gameplayNotesText != null) gameplayNotesText.text = unknown;
        }
    }

    private void OnEntryClick()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleEffectCoroutine());
    }

    private IEnumerator ScaleEffectCoroutine()
    {
        float t = 0f;
        Vector3 start = transform.localScale;
        Vector3 target = originalScale * hoverScale;
        while (t < hoverDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.PingPong(t / hoverDuration, 1f);
            transform.localScale = Vector3.Lerp(start, target, lerp);
            yield return null;
        }
        transform.localScale = originalScale;
    }
} 