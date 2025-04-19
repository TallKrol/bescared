using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int maxAmmo = 6; // Максимальное количество патронов в магазине
    public int currentAmmo; // Текущее количество патронов
    public int totalAmmo = 30; // Общее количество патронов игрока
    public float fireRate = 0.5f; // Скорость стрельбы (время между выстрелами)
    public float reloadTime = 2f; // Время перезарядки
    public float range = 50f; // Дальность выстрела
    public int damage = 20; // Урон от выстрела

    [Header("References")]
    public Camera fpsCamera; // Камера игрока
    public ParticleSystem muzzleFlash; // Эффект вспышки при стрельбе
    public AudioSource fireSound; // Звук выстрела
    public AudioSource reloadSound; // Звук перезарядки

    private bool isReloading = false; // Флаг перезарядки
    private float nextTimeToFire = 0f; // Время для следующего выстрела

    void Start()
    {
        currentAmmo = maxAmmo; // Заполняем магазин при старте
    }

    void Update()
    {
        // Оружие не стреляет, если идёт перезарядка
        if (isReloading) return;

        // Стрельба по нажатию левой кнопки мыши
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // Перезарядка по нажатию клавиши R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        // Уменьшаем текущие патроны
        currentAmmo--;

        // Эффект вспышки
        if (muzzleFlash != null) muzzleFlash.Play();

        // Звук выстрела
        if (fireSound != null) fireSound.Play();

        // Луч для проверки попадания
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            // Если у объекта есть здоровье, наносим урон
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        // Звук перезарядки
        if (reloadSound != null) reloadSound.Play();

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        // Пополняем магазин
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);
        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        isReloading = false;
    }
}