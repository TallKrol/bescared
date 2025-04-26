using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovementFirstPerson : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    public bool canDoubleJump = false;
    public AudioClip jumpSound;
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;

    private CharacterController _controller;
    private PlayerStats playerStats;
    private Vector3 velocity;
    private bool isCrouching = false;
    private float originalHeight;
    private float _fallVelocity = 0;
    public float crouchHeight = 1f;
    private float verticalLookRotation = 0f;
    private bool isGrounded;
    private bool hasDoubleJumped = false;
    private AudioSource audioSource;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>();
        originalHeight = _controller.height;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical"); // W/S

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && playerStats.currentStamina > 0;
        float speed = isRunning ? runSpeed : walkSpeed;
        if (isRunning)
        {
            playerStats.UseStamina(playerStats.staminaDrainRate * Time.deltaTime);
        }

        if (isCrouching) speed = crouchSpeed;

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        _controller.Move(move * speed * Time.deltaTime);

        // Проверка нахождения на земле
        isGrounded = _controller.isGrounded;
        
        if (isGrounded)
        {
            velocity.y = -0.5f; // Небольшая отрицательная скорость для лучшего прилипания к земле
        }
        
        // Обработка прыжка на пробел
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            PerformJump();
        }
        
        // Применение гравитации
        velocity.y += gravity * Time.deltaTime;
        
        // Перемещение
        _controller.Move(velocity * Time.deltaTime);

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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void PerformJump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        
        // Воспроизведение звука прыжка
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
}