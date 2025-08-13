using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public InputActionAsset inputActions;

    [Header("Movement Speeds")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;
    public float gravityUp = -15f;
    public float gravityDown = -30f;
    public float jumpCutMultiplier = 3f;  // Gravity multiplier when jump is released early
    public float airControlPercent = 0.8f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isCrouching = false;
    private bool isSprinting = false;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction jumpAction;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Ground check and reset vertical velocity if grounded
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;  // Small downward force to stick to ground
        }

        // Get raw input for analog precision
        float horizontal = moveAction.ReadValue<Vector2>().x;
        float vertical = moveAction.ReadValue<Vector2>().y;

        // Calculate input magnitude for speed scaling (analog stick)
        float inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);

        // Get camera-relative directions (flatten Y)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Direction vector for movement (normalized for rotation)
        Vector3 moveDirection = (camRight * horizontal + camForward * vertical).normalized;

        // Sprint check (Left Shift), disable sprint while crouching
        isSprinting = sprintAction.IsPressed() && !isCrouching;

        // Crouch toggle (Left Control)
        if (crouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : standingHeight;
        }

        // Determine current speed based on state
        float currentSpeed = walkSpeed;
        if (isSprinting) currentSpeed = sprintSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;

        // Adjust speed for analog stick magnitude and air control
        float finalSpeed = currentSpeed * (controller.isGrounded ? inputMagnitude : inputMagnitude * airControlPercent);
        Vector3 horizontalVelocity = moveDirection * finalSpeed;

        // Jump logic
        if (jumpAction.WasPressedThisFrame() && controller.isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityUp);
        }

        // Variable jump height � if jump released early, apply extra downward gravity
        if (!jumpAction.IsPressed() && velocity.y > 0 && !controller.isGrounded)
        {
            velocity.y += gravityUp * (jumpCutMultiplier - 1) * Time.deltaTime;
        }

        // Apply gravity (stronger when falling)
        if (velocity.y > 0)
        {
            velocity.y += gravityUp * Time.deltaTime;
        }
        else
        {
            velocity.y += gravityDown * Time.deltaTime;
        }

        // Move character controller (horizontal + vertical)
        controller.Move((horizontalVelocity + velocity) * Time.deltaTime);

        // Rotate player to face movement direction smoothly
        if (inputMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.15f);
        }
    }
}
