using UnityEngine;

[System.Serializable]
public class MerchantItem
{
    public string itemName;
    public GameObject itemPrefab;
    
    public enum TradeType
    {
        BuyFromMerchant,    // Игрок покупает у торговца
        SellToMerchant      // Игрок продает торговцу
    }
    
    public TradeType tradeType;
    
    [Header("Цены")]
    public int goldPrice;   // Золото (получает игрок при продаже или отдает при покупке)
    public int ammoPrice;   // Патроны (отдает игрок при продаже или получает при покупке)
    
    public string description;
    [Range(0f, 1f)]
    public float spawnChance = 1f;
}

public class MerchantItemDisplay : MonoBehaviour
{
    public MerchantItem item;
    private TextMesh priceText;
    private bool isHovered = false;

    private void Start()
    {
        priceText = GetComponentInChildren<TextMesh>();
        if (priceText != null)
        {
            priceText.text = "";
        }
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        if (priceText != null)
        {
            string priceInfo = item.tradeType == MerchantItem.TradeType.BuyFromMerchant ? 
                "Купить за:\n" : "Продать за:\n";
            
            if (item.goldPrice > 0)
            {
                if (item.tradeType == MerchantItem.TradeType.BuyFromMerchant)
                    priceInfo += $"{item.goldPrice} золота\n";
                else
                    priceInfo += $"{item.goldPrice} золота\n";
            }
            
            if (item.ammoPrice > 0)
            {
                if (item.tradeType == MerchantItem.TradeType.BuyFromMerchant)
                    priceInfo += $"{item.ammoPrice} патронов";
                else
                    priceInfo += $"{item.ammoPrice} патронов";
            }
            
            priceText.text = priceInfo;
        }
    }

    private void OnMouseExit()
    {
        isHovered = false;
        if (priceText != null)
        {
            priceText.text = "";
        }
    }

    public bool IsHovered()
    {
        return isHovered;
    }
} 