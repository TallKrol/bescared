using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public Sprite icon;
    public bool stackable;
    public int count = 1;
    public int maxStack = 1;
    public ItemType type;
    public string description;

    public enum ItemType
    {
        Weapon,
        Ammo,
        Consumable,
        Key,
        Blood,
        Other
    }

    public virtual void OnAddToInventory() { }
    public virtual void OnRemoveFromInventory() { }
    public virtual void Use() { }
} 