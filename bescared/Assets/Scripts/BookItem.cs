using UnityEngine;

public class BookItem : Item
{
    private MonsterBook bookManager;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeBookManager();
    }

    private void InitializeBookManager()
    {
        if (isInitialized) return;

        bookManager = MonsterBook.Instance;
        if (bookManager == null)
        {
            Debug.LogError("MonsterBook instance not found! Book functionality will be limited.");
        }
        else
        {
            isInitialized = true;
        }
    }

    public override void OnAddToInventory()
    {
        base.OnAddToInventory();
        InitializeBookManager();

        if (bookManager != null)
        {
            bookManager.SetInInventory(true);
        }
        else
        {
            Debug.LogWarning("Cannot add book to inventory: MonsterBook instance not found!");
        }
    }

    public override void OnRemoveFromInventory()
    {
        base.OnRemoveFromInventory();
        InitializeBookManager();

        if (bookManager != null)
        {
            bookManager.SetInInventory(false);
        }
        else
        {
            Debug.LogWarning("Cannot remove book from inventory: MonsterBook instance not found!");
        }
    }

    public override void Use()
    {
        if (BookUI.Instance == null)
        {
            Debug.LogError("Cannot use book: BookUI instance not found!");
            return;
        }

        BookUI.Instance.OpenBook();
    }
} 