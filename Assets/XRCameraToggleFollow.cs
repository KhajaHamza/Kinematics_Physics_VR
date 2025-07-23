using UnityEngine;
using UnityEngine.InputSystem;

public class XRCameraToggleFollow : MonoBehaviour
{
    public Transform acceleratedCar; // Assign your accelerated car
    public Transform xrRig;          // XR Origin
    public InputActionReference thumbstickInput; // Reference to thumbstick input action
    public float movementSpeed = 5f; // Speed of thumbstick movement

    private bool wasPaused = false;
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;

    void OnEnable()
    {
        if (thumbstickInput != null)
        {
            thumbstickInput.action.Enable();
        }
    }

    void OnDisable()
    {
        if (thumbstickInput != null)
        {
            thumbstickInput.action.Disable();
        }
    }

    void Update()
    {
        if (PauseManager.isSimulationPaused && !wasPaused)
        {
            // Save the XR Rig's local position and rotation before detaching
            if (xrRig.parent == acceleratedCar)
            {
                savedLocalPosition = xrRig.localPosition;
                savedLocalRotation = xrRig.localRotation;
            }

            // Detach XR Rig from car
            xrRig.parent = null;
            wasPaused = true;
        }
        else if (!PauseManager.isSimulationPaused && wasPaused)
        {
            // Re-attach XR Rig to car
            xrRig.parent = acceleratedCar;

            // Restore the saved local position and rotation
            xrRig.localPosition = savedLocalPosition;
            xrRig.localRotation = savedLocalRotation;

            wasPaused = false;
        }

        // Handle thumbstick movement when paused
        if (PauseManager.isSimulationPaused)
        {
            HandleThumbstickMovement();
        }
    }

    void HandleThumbstickMovement()
    {
        if (thumbstickInput == null) return;

        // Read thumbstick input (Vector2)
        Vector2 input = thumbstickInput.action.ReadValue<Vector2>();
        if (input.magnitude > 0.1f) // Deadzone to prevent drift
        {
            // Get the XR Rig's forward and right vectors (ignoring pitch)
            Vector3 forward = xrRig.forward;
            forward.y = 0; // Keep movement horizontal
            forward.Normalize();
            Vector3 right = xrRig.right;
            right.y = 0;
            right.Normalize();

            // Calculate movement direction
            Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
            Vector3 movement = moveDirection * movementSpeed * Time.unscaledDeltaTime;

            // Move the XR Rig
            xrRig.position += movement;
        }
    }
}