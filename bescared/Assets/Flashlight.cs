using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight; // Свет фонарика
    public float damagePerSecond = 10f; // Урон в секунду от света
    public float batteryLife = 100f; // Максимальный заряд батареи
    public float batteryDrainRate = 5f; // Скорость разряда батареи
    public float batteryRechargeRate = 2f; // Скорость зарядки батареи
    public bool isRecharging = false; // Флаг зарядки
    public LayerMask lightSensitiveMonstersLayer; // Слой монстров, чувствительных к свету

    [Header("Light Properties")]
    public float defaultRange = 10f; // Стандартная дальность света
    public float defaultIntensity = 1f; // Стандартная интенсивность света
    public Color defaultColor = Color.white; // Стандартный цвет света

    [Header("Flicker Settings")]
    public float flickerThreshold = 20f; // Порог заряда, при котором начинает мерцать
    public float minFlickerIntensity = 0.2f; // Минимальная интенсивность при мерцании
    public float flickerSpeed = 5f; // Скорость мерцания
    public float flickerRandomness = 0.2f; // Случайность мерцания

    private bool isOn = false; // Состояние фонарика
    private Collider[] hitColliders; // Массив для хранения коллайдеров в радиусе света
    private float flickerTimer = 0f; // Таймер для мерцания

    private void Start()
    {
        // Инициализация настроек света для фонарика
        flashlight.range = defaultRange;
        flashlight.intensity = defaultIntensity;
        flashlight.color = defaultColor;
        flashlight.enabled = false; // Гарантируем, что фонарик выключен при старте
    }

    private void Update()
    {
        // Включение/выключение фонарика по клавише F
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        // Если фонарик включен, расходуем заряд
        if (isOn)
        {
            DrainBattery();
            DamageEnemiesInLight();
            UpdateFlicker();
        }
        else if (!isRecharging && batteryLife < 100f)
        {
            RechargeBattery();
        }
    }

    private void ToggleFlashlight()
    {
        if (batteryLife <= 0)
        {
            isOn = false;
            flashlight.enabled = false;
            return;
        }

        isOn = !isOn;
        flashlight.enabled = isOn;
    }

    private void DrainBattery()
    {
        if (batteryLife > 0)
        {
            batteryLife -= batteryDrainRate * Time.deltaTime;
            if (batteryLife <= 0)
            {
                batteryLife = 0;
                isOn = false;
                flashlight.enabled = false;
            }
        }
        else
        {
            isOn = false;
            flashlight.enabled = false;
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
        // Находим монстров в радиусе света
        hitColliders = Physics.OverlapSphere(flashlight.transform.position, flashlight.range, lightSensitiveMonstersLayer);
        foreach (var hitCollider in hitColliders)
        {
            TargetWithLight target = hitCollider.GetComponent<TargetWithLight>();
            if (target != null)
            {
                target.TakeLightDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    private void UpdateFlicker()
    {
        if (batteryLife <= flickerThreshold)
        {
            // Обновляем таймер мерцания
            flickerTimer += Time.deltaTime * flickerSpeed;
            
            // Добавляем случайность к мерцанию
            float randomOffset = Random.Range(-flickerRandomness, flickerRandomness);
            float flickerValue = Mathf.PingPong(flickerTimer + randomOffset, 1f);
            
            // Интерполируем интенсивность между минимальной и стандартной
            float currentIntensity = Mathf.Lerp(minFlickerIntensity, defaultIntensity, flickerValue);
            flashlight.intensity = currentIntensity;
        }
        else
        {
            // Если заряд выше порога, возвращаем стандартную интенсивность
            flashlight.intensity = defaultIntensity;
        }
    }

    // Методы для изменения настроек света
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