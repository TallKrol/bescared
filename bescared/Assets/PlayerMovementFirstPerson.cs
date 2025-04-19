using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))] // ������� ������� PlayerStats
public class PlayerMovementFirstPerson : MonoBehaviour
{
    public float walkSpeed = 5f; // �������� ������
    public float runSpeed = 10f; // �������� ����
    public float crouchSpeed = 2f; // �������� ��� ����������
    public float jumpHeight = -2f; // ������ ������
    public float gravity = 9.81f; // ����������
    public Transform cameraTransform; // ������ �� ������
    public float mouseSensitivity = 100f; // ���������������� ����

    private CharacterController _controller;
    private PlayerStats playerStats; // ������ �� ������ PlayerStats
    private Vector3 velocity;
    private bool isCrouching = false;
    private float originalHeight;
    private float _fallVelocity = 0;
    public float crouchHeight = 1f; // ������ ��� ����������
    private float verticalLookRotation = 0f; // ��� ������������� �������� ������

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        playerStats = GetComponent<PlayerStats>(); // �������� ������ �� PlayerStats
        originalHeight = _controller.height;
        Cursor.lockState = CursorLockMode.Locked; // ��������� ������ ��� ���������� �����
    }

    private void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    private void MovePlayer()
    {
        // �������� ���� �� ������ WASD
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical"); // W/S

        // ���������� ������� ��������
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && playerStats.currentStamina > 0;
        float speed = isRunning ? runSpeed : walkSpeed;
        if (isRunning)
        {
            playerStats.UseStamina(playerStats.staminaDrainRate * Time.deltaTime);
        }

        if (isCrouching) speed = crouchSpeed; // ��������� �������� ��� ����������

        // ������������ �������� � ������������ ������
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        _controller.Move(move * speed * Time.deltaTime);

        // ������
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

        // ����������
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
        // �������� ������� ������ �� ����
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �������� ������ �� ����������� (����/�����)
        transform.Rotate(Vector3.up * mouseX);

        // �������� ������ �� ��������� (�����/����)
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // ������������ ���� ������
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }
}