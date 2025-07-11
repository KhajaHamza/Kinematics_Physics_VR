using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public InputActionAsset inputActions; // Assign this in Inspector
    private InputAction pauseAction;

    public static bool isSimulationPaused = false; // Use this in other scripts

    void OnEnable()
    {
        pauseAction = inputActions.FindActionMap("VRControls")?.FindAction("Pause");

        if (pauseAction != null)
            pauseAction.Enable();
        else
            Debug.LogError("Pause action not found!");
    }

    void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.Disable();
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            isSimulationPaused = !isSimulationPaused;
            Debug.Log(isSimulationPaused ? "Simulation Paused" : "Simulation Resumed");
        }
    }
}
