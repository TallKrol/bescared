using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int maxAmmo = 6; // Максимальное количество патронов в обойме
    public int currentAmmo; // Текущее количество патронов в обойме
    public int totalAmmo = 30; // Общее количество патронов
    public int maxTotalAmmo = 54; // Максимальное общее количество патронов
    public float fireRate = 0.5f; // (ск) Время между выстрелами в секундах
    public float reloadTime = 2f; // Время перезарядки в секундах
    public float range = 50f; // Дальность стрельбы в юнитах
    public int damage = 20; // Урон по целям

    [Header("Aiming Settings")]
    public float aimSpeed = 10f; // Скорость прицеливания (чем больше, тем быстрее)
    public Vector3 hipPosition; // Позиция оружия при стрельбе от бедра
    public Vector3 aimPosition; // Позиция оружия при прицеливании
    public float aimFOV = 40f; // Угол обзора при прицеливании
    public float normalFOV = 60f; // Обычный угол обзора

    [Header("Bullet Settings")]
    public AnimationCurve bulletDropCurve; // Кривая падения пули (настраивается в инспекторе)
    public float bulletSpeed = 100f; // Скорость пули в юнитах в секунду
    public float maxBulletDrop = 5f; // Максимальное падение пули в юнитах

    [Header("References")]
    public Camera fpsCamera; // Камера от первого лица
    public ParticleSystem muzzleFlash; // Эффект вспышки при выстреле
    public ParticleSystem smokeEffect; // Эффект дыма после выстрела
    public AudioSource fireSound; // Звук выстрела
    public AudioSource reloadSound; // Звук перезарядки
    public Transform weaponTransform; // Transform оружия для анимации

    // Приватные переменные состояния
    private bool isReloading = false; // Флаг перезарядки
    private float nextTimeToFire = 0f; // Время следующего возможного выстрела
    private bool isAiming = false; // Флаг прицеливания
    private float currentFOV; // Текущий угол обзора
    private Vector3 targetPosition; // Целевая позиция оружия
    private Quaternion targetRotation; // Целевой поворот оружия

    // Пул объектов для оптимизации
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private int poolSize = 20; // Размер пула пуль

    void Start()
    {
        // Инициализация начальных значений
        currentAmmo = maxAmmo;
        currentFOV = normalFOV;
        targetPosition = hipPosition;
        targetRotation = Quaternion.identity;

        // Создание пула пуль
        InitializeBulletPool();
    }

    void Update()
    {
        // Если перезаряжаемся - выходим
        if (isReloading) return;

        // Обработка прицеливания
        HandleAiming();

        // Стрельба по левой кнопке мыши
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // Перезарядка по кнопке R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }

        // Плавное перемещение оружия
        UpdateWeaponPosition();
    }

    // Обработка прицеливания
    private void HandleAiming()
    {
        // Прицеливание по правой кнопке мыши
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
            targetPosition = aimPosition;
            // Плавное изменение FOV
            currentFOV = Mathf.Lerp(currentFOV, aimFOV, Time.deltaTime * aimSpeed);
        }
        else
        {
            isAiming = false;
            targetPosition = hipPosition;
            // Возврат к обычному FOV
            currentFOV = Mathf.Lerp(currentFOV, normalFOV, Time.deltaTime * aimSpeed);
        }

        // Применяем новый FOV к камере
        fpsCamera.fieldOfView = currentFOV;
    }

    // Плавное перемещение оружия
    private void UpdateWeaponPosition()
    {
        weaponTransform.localPosition = Vector3.Lerp(
            weaponTransform.localPosition,
            targetPosition,
            Time.deltaTime * aimSpeed
        );
    }

    // Процесс выстрела
    private void Shoot()
    {
        // Уменьшаем патроны
        currentAmmo--;

        // Визуальные эффекты
        if (muzzleFlash != null) muzzleFlash.Play();
        if (smokeEffect != null) smokeEffect.Play();
        if (fireSound != null) fireSound.Play();

        // Создаём визуальную пулю из пула
        GameObject bullet = GetBulletFromPool();
        if (bullet != null)
        {
            bullet.transform.position = fpsCamera.transform.position;
            bullet.transform.rotation = fpsCamera.transform.rotation;
            bullet.SetActive(true);
            StartCoroutine(MoveBullet(bullet));
        }

        // Проверка попадания
        RaycastHit hit;
        Vector3 direction = CalculateBulletDirection();
        if (Physics.Raycast(fpsCamera.transform.position, direction, out hit, range))
        {
            HandleHit(hit);
        }
    }

    // Расчёт траектории пули с учётом падения
    private Vector3 CalculateBulletDirection()
    {
        Vector3 direction = fpsCamera.transform.forward;
        float distance = Vector3.Distance(fpsCamera.transform.position, 
            fpsCamera.transform.position + direction * range);
        // Используем кривую для расчёта падения
        float drop = bulletDropCurve.Evaluate(distance / range) * maxBulletDrop;
        direction.y -= drop;
        return direction;
    }

    // Обработка попадания
    private void HandleHit(RaycastHit hit)
    {
        // Проверка на углового монстра
        CornerMonster cornerMonster = hit.transform.GetComponent<CornerMonster>();
        if (cornerMonster != null)
        {
            cornerMonster.OnHit();
            return;
        }

        // Проверка на обычную цель
        Target target = hit.transform.GetComponent<Target>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }

    // Анимация полёта пули
    private System.Collections.IEnumerator MoveBullet(GameObject bullet)
    {
        float distance = 0f;
        Vector3 startPosition = bullet.transform.position;
        Vector3 direction = CalculateBulletDirection();

        // Движение пули до достижения максимальной дальности
        while (distance < range)
        {
            distance += bulletSpeed * Time.deltaTime;
            bullet.transform.position = startPosition + direction * distance;
            yield return null;
        }

        // Возврат пули в пул
        ReturnBulletToPool(bullet);
    }

    // Инициализация пула пуль
    private void InitializeBulletPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = new GameObject("Bullet");
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    // Получение пули из пула
    private GameObject GetBulletFromPool()
    {
        if (bulletPool.Count > 0)
        {
            return bulletPool.Dequeue();
        }
        return null;
    }

    // Возврат пули в пул
    private void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    // Процесс перезарядки
    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        if (reloadSound != null) reloadSound.Play();

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        // Расчёт количества патронов для перезарядки
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);
        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        isReloading = false;
    }

    // Добавление патронов (например, при подборе)
    public void AddAmmo(int amount)
    {
        totalAmmo = Mathf.Min(totalAmmo + amount, maxTotalAmmo);
    }
}