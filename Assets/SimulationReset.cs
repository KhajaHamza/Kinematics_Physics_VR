using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SimulationReset : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction resetAction;

    void OnEnable()
    {
        resetAction = inputActions.FindActionMap("VRControls")?.FindAction("Reset");
        if (resetAction != null) resetAction.Enable();
    }

    void OnDisable()
    {
        if (resetAction != null) resetAction.Disable();
    }

    void Update()
    {
        if (resetAction != null && resetAction.WasPressedThisFrame())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
