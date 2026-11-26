using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxPitch = 89f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;
    private bool cursorLocked;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // The camera is expected to be a child of the player, but fall back to the main camera.
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        LockCursor(true);
    }

    private void OnDisable()
    {
        LockCursor(false);
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (Mouse.current == null)
            return;

        if (!cursorLocked)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                LockCursor(true);

            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        // Yaw rotates the whole body, pitch only the camera.
        transform.Rotate(Vector3.up * mouseDelta.x);

        pitch = Mathf.Clamp(pitch - mouseDelta.y, -maxPitch, maxPitch);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
            return;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input.x += 1f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input.x -= 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        Vector3 horizontalMove = (transform.right * input.x + transform.forward * input.y) * speed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        controller.Move((horizontalMove + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void LockCursor(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
