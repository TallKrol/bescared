using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f; // ������������ ������� ��������
    public float minSanity = -25f; // ����������� ������� ��������
    public float currentSanity; // ������� ������� ��������
    public float sanityDrainRate = 5f; // �������� ���������� �������� (��������, � �������)
    public float sanityRecoveryRate = 3f; // �������� �������������� �������� (��������, �� �����)

    [Header("Light Check")]
    public LayerMask lightLayer; // ���� ����� ��� ��������
    public float lightCheckRadius = 5f; // ������ �������� ����� ������ ������

    private bool isInLight = false; // ��������� �� ����� � ���� �����

    private void Start()
    {
        // ������������� ��������� �������� ��������
        currentSanity = maxSanity;
    }

    private void Update()
    {
        CheckLight();

        if (isInLight)
        {
            RecoverSanity();
        }
        else
        {
            DrainSanity();
        }

        // ������������ �������� �������� � �������� �� -25 �� 100
        currentSanity = Mathf.Clamp(currentSanity, minSanity, maxSanity);
    }

    private void CheckLight()
    {
        // ��������, ��������� �� ����� � ���� �����
        Collider[] lightSources = Physics.OverlapSphere(transform.position, lightCheckRadius, lightLayer);
        isInLight = lightSources.Length > 0;
    }

    private void DrainSanity()
    {
        // ���������� ��������
        currentSanity -= sanityDrainRate * Time.deltaTime;
    }

    private void RecoverSanity()
    {
        // �������������� ��������
        currentSanity += sanityRecoveryRate * Time.deltaTime;
    }

    public void TakeSanityDamage(float amount)
    {
        // ���������� �������� �� ������� ��������
        currentSanity -= amount;
    }

    public void RestoreSanity(float amount)
    {
        // �������������� �������� �� ������� ��������
        currentSanity += amount;
    }
}
