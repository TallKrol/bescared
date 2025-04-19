using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight; // Сам источник света
    public float damagePerSecond = 10f; // Урон от света для врагов
    public float batteryLife = 100f; // Максимальный заряд батареи
    public float batteryDrainRate = 5f; // Скорость разряда батареи
    public float batteryRechargeRate = 2f; // Скорость зарядки батареи
    public bool isRecharging = false; // Флаг для зарядки
    public LayerMask enemyLayer; // Слой врагов, чтобы свет мог взаимодействовать с ними

    [Header("Light Properties")]
    public float defaultRange = 10f; // Дальность света
    public float defaultIntensity = 1f; // Яркость света
    public Color defaultColor = Color.white; // Цвет света

    private bool isOn = false; // Включён ли фонарик
    private Collider[] hitColliders; // Для нахождения врагов в зоне света

    private void Start()
    {
        // Настраиваем параметры света при старте
        flashlight.range = defaultRange;
        flashlight.intensity = defaultIntensity;
        flashlight.color = defaultColor;
    }

    private void Update()
    {
        // Включение/выключение фонарика по нажатию F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        // Если фонарик включён, разряжаем батарею
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
            flashlight.enabled = isOn; // Включаем/выключаем источник света
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
                ToggleFlashlight(); // Выключаем фонарик, если батарея разряжена
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
        // Проверяем врагов в зоне света
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

    // Методы для изменения параметров света
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