using UnityEngine;

public class MerchantBell : MonoBehaviour
{
    public Merchant merchant;
    public float cooldownTime = 1f;
    private float lastRingTime;
    private bool canRing = true;

    private void OnMouseDown()
    {
        if (canRing && Time.time - lastRingTime >= cooldownTime)
        {
            RingBell();
            lastRingTime = Time.time;
        }
    }

    private void RingBell()
    {
        if (merchant != null)
        {
            merchant.RingBell();
        }
    }
} 