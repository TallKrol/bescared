using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))] // Требуем наличие PlayerStats
public class PlayerMovementFirstPerson : MonoBehaviour
{
    public float walkSpeed = 5f; // Скорость ходьбы
    public float runSpeed = 10f; // Скорость бега
    public float crouchSpeed = 2f; // Скорость при приседании
    public float jumpHeight = -2f; // Высота прыжка
    public float gravity = 9.81f; // Гравитация
    public Transform cameraTransform; // Ссылка на камеру
    public float mouseSensitivity = 100f; // Чувствительность мыши

    private CharacterController _controller;
    private PlayerStats playerStats; // Ссылка на скрипт PlayerStats
    private Vector3 velocity;
    private bool isCrouching = false;
    private float originalHeight;
    private float _fallVelocity = 0;
    public float crouchHeight = 1f; // Высота при приседании
    private float verticalLookRotation = 0f; // Для вертикального вращения камеры

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>(); // Получаем ссылку на PlayerStats
        originalHeight = _controller.height;
        Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор для управления мышью
    }

    private void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    private void MovePlayer()
    {
        // Получаем ввод от клавиш WASD
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical"); // W/S

        // Определяем текущую скорость
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && playerStats.currentStamina > 0;
        float speed = isRunning ? runSpeed : walkSpeed;
        if (isRunning)
        {
            playerStats.UseStamina(playerStats.staminaDrainRate * Time.deltaTime);
        }

        if (isCrouching) speed = crouchSpeed; // Уменьшаем скорость при приседании

        // Рассчитываем движение в пространстве игрока
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        _controller.Move(move * speed * Time.deltaTime);

        // Прыжок
        if (Input.GetKey(KeyCode.Space) && _controller.isGrounded)
        {
            _fallVelocity = -jumpHeight;
        }

        _fallVelocity += gravity * Time.fixedDeltaTime;
        _controller.Move(Vector3.down * _fallVelocity * Time.fixedDeltaTime);
        
        if (_controller.isGrounded)
        {
            _fallVelocity = 0;
        }

        _controller.Move(velocity * Time.deltaTime);

        // Приседание
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            _controller.height = crouchHeight;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            _controller.height = originalHeight;
        }
    }

    private void RotateCamera()
    {
        // Получаем входные данные от мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Вращение игрока по горизонтали (лево/право)
        transform.Rotate(Vector3.up * mouseX);

        // Вращение камеры по вертикали (вверх/вниз)
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Ограничиваем угол обзора
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }
}