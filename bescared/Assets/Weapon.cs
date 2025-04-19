using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int maxAmmo = 6; // ������������ ���������� �������� � ��������
    public int currentAmmo; // ������� ���������� ��������
    public int totalAmmo = 30; // ����� ���������� �������� ������
    public float fireRate = 0.5f; // �������� �������� (����� ����� ����������)
    public float reloadTime = 2f; // ����� �����������
    public float range = 50f; // ��������� ��������
    public int damage = 20; // ���� �� ��������

    [Header("References")]
    public Camera fpsCamera; // ������ ������
    public ParticleSystem muzzleFlash; // ������ ������� ��� ��������
    public AudioSource fireSound; // ���� ��������
    public AudioSource reloadSound; // ���� �����������

    private bool isReloading = false; // ���� �����������
    private float nextTimeToFire = 0f; // ����� ��� ���������� ��������

    void Start()
    {
        currentAmmo = maxAmmo; // ��������� ������� ��� ������
    }

    void Update()
    {
        // ������ �� ��������, ���� ��� �����������
        if (isReloading) return;

        // �������� �� ������� ����� ������ ����
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // ����������� �� ������� ������� R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        // ��������� ������� �������
        currentAmmo--;

        // ������ �������
        if (muzzleFlash != null) muzzleFlash.Play();

        // ���� ��������
        if (fireSound != null) fireSound.Play();

        // ��� ��� �������� ���������
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            // ���� � ������� ���� ��������, ������� ����
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

        // ���� �����������
        if (reloadSound != null) reloadSound.Play();

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);

        // ��������� �������
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);
        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        isReloading = false;
    }
}