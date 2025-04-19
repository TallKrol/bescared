using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight; // ��� �������� �����
    public float damagePerSecond = 10f; // ���� �� ����� ��� ������
    public float batteryLife = 100f; // ������������ ����� �������
    public float batteryDrainRate = 5f; // �������� ������� �������
    public float batteryRechargeRate = 2f; // �������� ������� �������
    public bool isRecharging = false; // ���� ��� �������
    public LayerMask enemyLayer; // ���� ������, ����� ���� ��� ����������������� � ����

    [Header("Light Properties")]
    public float defaultRange = 10f; // ��������� �����
    public float defaultIntensity = 1f; // ������� �����
    public Color defaultColor = Color.white; // ���� �����

    private bool isOn = false; // ������� �� �������
    private Collider[] hitColliders; // ��� ���������� ������ � ���� �����

    private void Start()
    {
        // ����������� ��������� ����� ��� ������
        flashlight.range = defaultRange;
        flashlight.intensity = defaultIntensity;
        flashlight.color = defaultColor;
    }

    private void Update()
    {
        // ���������/���������� �������� �� ������� F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        // ���� ������� �������, ��������� �������
        if (isOn)
        {
            DrainBattery();
            DamageEnemiesInLight();
        }
        else if (!isRecharging && batteryLife < 100f)
        {
            RechargeBattery();
        }
    }

    private void ToggleFlashlight()
    {
        if (batteryLife > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn; // ��������/��������� �������� �����
        }
    }

    private void DrainBattery()
    {
        if (batteryLife > 0)
        {
            batteryLife -= batteryDrainRate * Time.deltaTime;
            if (batteryLife <= 0)
            {
                batteryLife = 0;
                ToggleFlashlight(); // ��������� �������, ���� ������� ���������
            }
        }
    }

    private void RechargeBattery()
    {
        batteryLife += batteryRechargeRate * Time.deltaTime;
        if (batteryLife > 100f)
        {
            batteryLife = 100f;
        }
    }

    private void DamageEnemiesInLight()
    {
        // ��������� ������ � ���� �����
        hitColliders = Physics.OverlapSphere(flashlight.transform.position, flashlight.range, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            TargetWithLight target = hitCollider.GetComponent<TargetWithLight>();
            if (target != null)
            {
                target.TakeLightDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    // ������ ��� ��������� ���������� �����
    public void SetLightRange(float range)
    {
        flashlight.range = range;
    }

    public void SetLightIntensity(float intensity)
    {
        flashlight.intensity = intensity;
    }

    public void SetLightColor(Color color)
    {
        flashlight.color = color;
    }

    public void ResetLightSettings()
    {
        flashlight.range = defaultRange;
        flashlight.intensity = defaultIntensity;
        flashlight.color = defaultColor;
    }
}