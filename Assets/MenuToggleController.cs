using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggleController : MonoBehaviour
{
    public GameObject menuCanvas; // Assign your World Space canvas
    public InputActionReference toggleMenuAction; // Assign from XRControls
    public Transform vrCamera; // Assign the XR Rig's Main Camera here
    public float distanceFromCamera = 2f; // How far in front of the camera

    private void OnEnable()
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += ToggleMenu;
    }

    private void OnDisable()
    {
        toggleMenuAction.action.performed -= ToggleMenu;
        toggleMenuAction.action.Disable();
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        bool isActive = !menuCanvas.activeSelf;
        menuCanvas.SetActive(isActive);

        if (isActive)
        {
            PositionMenuInFrontOfCamera();
        }
    }

    private void PositionMenuInFrontOfCamera()
    {
        Vector3 cameraForward = vrCamera.forward;
        Vector3 cameraPosition = vrCamera.position;

        Vector3 targetPosition = cameraPosition + cameraForward.normalized * distanceFromCamera;
        targetPosition.y = vrCamera.position.y; // Optional: Keep it level with headset

        menuCanvas.transform.position = targetPosition;

        // Face the menu toward the user
        menuCanvas.transform.rotation = Quaternion.LookRotation(cameraForward);
    }
}
