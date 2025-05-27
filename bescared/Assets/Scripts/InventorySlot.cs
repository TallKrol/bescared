using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public TextMeshProUGUI itemCount;
    public Item currentItem;
    public int slotIndex;
    public bool isHotbarSlot;
    public bool isLockedSlot; // Для оружия, фонарика и крови
    public Item.ItemType allowedItemType; // Тип предмета, который можно положить в этот слот

    private static GameObject draggedItem;
    private static InventorySlot draggedSlot;
    private Vector3 originalPosition;

    private void Start()
    {
        if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }
        if (itemCount != null)
        {
            itemCount.text = "";
        }
    }

    public void SetItem(Item item)
    {
        if (isLockedSlot && item != null && item.type != allowedItemType)
        {
            Debug.LogWarning($"Нельзя положить предмет типа {item.type} в закрепленный слот для {allowedItemType}");
            return;
        }

        currentItem = item;
        UpdateVisuals();
    }

    public void ClearSlot()
    {
        currentItem = null;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = true;
            
            if (currentItem.stackable && currentItem.count > 1)
            {
                itemCount.text = currentItem.count.ToString();
            }
            else
            {
                itemCount.text = "";
            }
        }
        else
        {
            itemIcon.enabled = false;
            itemCount.text = "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || isLockedSlot) return;

        draggedItem = new GameObject("DraggedItem");
        draggedItem.transform.SetParent(transform.root);
        draggedItem.transform.SetAsLastSibling();

        Image draggedImage = draggedItem.AddComponent<Image>();
        draggedImage.sprite = itemIcon.sprite;
        draggedImage.raycastTarget = false;

        draggedSlot = this;
        originalPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            // Проверяем, не пытаемся ли мы выбросить предмет
            if (IsOverDropZone(eventData.position))
            {
                DropItem();
            }
            else
            {
                // Возвращаем предмет на место
                draggedItem.transform.position = originalPosition;
            }

            Destroy(draggedItem);
            draggedItem = null;
            draggedSlot = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;

        // Проверяем, можно ли положить предмет в этот слот
        if (isLockedSlot && draggedSlot.currentItem.type != allowedItemType)
        {
            return;
        }

        // Меняем местами предметы
        Item tempItem = currentItem;
        SetItem(draggedSlot.currentItem);
        draggedSlot.SetItem(tempItem);
    }

    private bool IsOverDropZone(Vector2 position)
    {
        // Проверяем, находится ли курсор за пределами инвентаря
        RectTransform inventoryRect = PlayerInventory.Instance.inventoryPanel.GetComponent<RectTransform>();
        return !RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, position);
    }

    private void DropItem()
    {
        if (currentItem == null) return;

        // Создаем предмет в мире
        Vector3 dropPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dropPosition.z = 0;
        
        GameObject droppedItem = new GameObject(currentItem.itemName);
        droppedItem.transform.position = dropPosition;
        
        // Добавляем компоненты для подбора
        SpriteRenderer spriteRenderer = droppedItem.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = currentItem.icon;
        
        BoxCollider2D collider = droppedItem.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        
        ItemPickup pickup = droppedItem.AddComponent<ItemPickup>();
        pickup.item = currentItem;

        // Очищаем слот
        ClearSlot();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.ShowTooltip(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideTooltip();
        }
    }
} 