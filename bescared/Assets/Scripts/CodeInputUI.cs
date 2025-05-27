using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeInputUI : MonoBehaviour
{
    public static CodeInputUI Instance { get; private set; }

    [Header("Элементы интерфейса")]
    public GameObject codeInputPanel;
    public TextMeshProUGUI codeDisplay;
    public Button[] numberButtons;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI hintText;

    private Door currentDoor;
    private string currentCode = "";

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
        // Инициализация кнопок
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int number = i;
            numberButtons[i].onClick.AddListener(() => AddNumber(number));
        }

        confirmButton.onClick.AddListener(ConfirmCode);
        cancelButton.onClick.AddListener(CancelInput);

        // Скрываем панель при старте
        codeInputPanel.SetActive(false);
    }

    public void ShowCodeInput(Door door)
    {
        currentDoor = door;
        currentCode = "";
        UpdateCodeDisplay();
        codeInputPanel.SetActive(true);
        hintText.text = "Введите 4-значный код";
    }

    private void AddNumber(int number)
    {
        if (currentCode.Length < 4)
        {
            currentCode += number.ToString();
            UpdateCodeDisplay();
        }
    }

    private void UpdateCodeDisplay()
    {
        codeDisplay.text = currentCode.PadRight(4, '*');
    }

    private void ConfirmCode()
    {
        if (currentCode.Length == 4)
        {
            if (currentCode == currentDoor.doorCode)
            {
                // Правильный код
                currentDoor.UnlockDoor();
                HideCodeInput();
            }
            else
            {
                // Неправильный код
                currentCode = "";
                UpdateCodeDisplay();
                hintText.text = "Неверный код!";
            }
        }
    }

    private void CancelInput()
    {
        HideCodeInput();
    }

    private void HideCodeInput()
    {
        codeInputPanel.SetActive(false);
        currentDoor = null;
        currentCode = "";
    }

    private void Update()
    {
        // Закрываем панель при нажатии Escape
        if (Input.GetKeyDown(KeyCode.Escape) && codeInputPanel.activeSelf)
        {
            HideCodeInput();
        }
    }
} 