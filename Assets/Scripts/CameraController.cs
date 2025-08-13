using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;             // Player to follow
    public float distance = 5.0f;         // Default distance from player
    public float minDistance = 1.0f;      // Closest distance when camera is blocked
    public float mouseSensitivity = 3.0f;
    public float verticalClamp = 80f;     // Max up/down rotation angle
    public float collisionBuffer = 0.2f;  // Extra space from walls
    public LayerMask collisionLayers;     // Layers the camera can collide with

    private float yaw;   // Left/right rotation
    private float pitch; // Up/down rotation
    private float currentDistance;
    
    private InputAction lookAction;

    private void Awake()
    {
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned!");
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentDistance = distance;
    }

    void LateUpdate()
    {
        // Get mouse movement input
        float mouseX = lookAction.ReadValue<Vector2>().x * mouseSensitivity / 10;
        float mouseY = lookAction.ReadValue<Vector2>().y * mouseSensitivity / 10;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Camera collision check
        Vector3 desiredPos = target.position + Vector3.up * 1.5f - (rotation * Vector3.forward * distance);

        if (Physics.Linecast(target.position + Vector3.up * 1.5f, desiredPos, out RaycastHit hit, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, distance);
        }
        else
        {
            currentDistance = distance;
        }

        // Apply camera position
        Vector3 finalPos = target.position + Vector3.up * 1.5f - (rotation * Vector3.forward * currentDistance);
        transform.position = finalPos;
        transform.rotation = rotation;
    }
}
