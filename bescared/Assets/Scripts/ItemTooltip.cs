using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private RectTransform tooltipRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            // Следуем за курсором мыши
            Vector2 mousePosition = Input.mousePosition;
            
            // Получаем размеры экрана
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // Получаем размеры тултипа
            float tooltipWidth = tooltipRect.rect.width;
            float tooltipHeight = tooltipRect.rect.height;
            
            // Вычисляем позицию тултипа, чтобы он не выходил за пределы экрана
            float x = Mathf.Clamp(mousePosition.x + 20, 0, screenWidth - tooltipWidth);
            float y = Mathf.Clamp(mousePosition.y + 20, 0, screenHeight - tooltipHeight);
            
            tooltipRect.position = new Vector2(x, y);
        }
    }

    public void ShowTooltip(Item item)
    {
        if (tooltipPanel == null || itemNameText == null || item == null) return;

        itemNameText.text = item.itemName;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
} 