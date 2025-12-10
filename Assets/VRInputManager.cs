using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputManager : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("References")]
    public GameObject menuCanvas;
    public Transform vrCamera;
    public Transform xrRig;
    public Transform followCar;  // The currently active car

    [Header("External Managers")]
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GraphManager graphManager;


    // Settings
    public float distanceFromCamera = 2f;
    public float movementSpeed = 135f;

    [Header("Camera Position Settings")]
    public Vector3 cameraFixedPosition = new Vector3(0f, 2f, -8f);
    public Vector3 cameraFixedRotation = new Vector3(0f, 0f, 0f);

    // Input Actions
    private InputAction pauseAction;
    private InputAction resetAction;
    private InputAction toggleMenuAction;
    private InputAction moveWhenPausedAction;
    private InputAction toggleGraphsAction;

    void Start()
    {
        // Position camera at fixed viewpoint at start
        if (xrRig != null)
        {
            xrRig.position = cameraFixedPosition;
        }

        // AUTO-DETECT which car is active at scene start
        if (menuManager != null)
        {
            if (menuManager.uaCar != null && menuManager.uaCar.gameObject.activeSelf)
            {
                SetFollowCar(menuManager.uaCar.transform);
                Debug.Log("Scene Start: Following UA Car (Car1)");
            }
            else if (menuManager.cvCar != null && menuManager.cvCar.gameObject.activeSelf)
            {
                SetFollowCar(menuManager.cvCar.transform);
                Debug.Log("Scene Start: Following CV Car (Car2)");
            }
            else
            {
                Debug.LogWarning("No active car found at scene start! Follow functionality will not work until you open the menu.");
            }
        }
        else
        {
            Debug.LogError("MenuManager reference is missing! Assign it in the Inspector.");
        }
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            pauseAction = inputActions.FindActionMap("VRControls")?.FindAction("Pause");
            resetAction = inputActions.FindActionMap("VRControls")?.FindAction("Reset");
            toggleMenuAction = inputActions.FindActionMap("VRControls")?.FindAction("ToggleMenu");
            moveWhenPausedAction = inputActions.FindActionMap("VRControls")?.FindAction("MoveWhenPaused");
            toggleGraphsAction = inputActions.FindActionMap("VRControls")?.FindAction("ToggleGraphs");

            if (pauseAction != null) pauseAction.Enable();
            if (resetAction != null) resetAction.Enable();
            if (toggleMenuAction != null) toggleMenuAction.Enable();
            if (moveWhenPausedAction != null) moveWhenPausedAction.Enable();
            if (toggleGraphsAction != null) toggleGraphsAction.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null) pauseAction.Disable();
        if (resetAction != null) resetAction.Disable();
        if (toggleMenuAction != null) toggleMenuAction.Disable();
        if (moveWhenPausedAction != null) moveWhenPausedAction.Disable();
        if (toggleGraphsAction != null) toggleGraphsAction.Disable();
    }

    void Update()
    {
        HandlePauseInput();
        HandleResetInput();
        HandleMenuToggleInput();
        HandleMovementWhenPaused();
        HandleToggleGraphsInput();
    }

    void HandlePauseInput()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            PauseManager.SetPaused(!PauseManager.isSimulationPaused);
            Debug.Log(PauseManager.isSimulationPaused ? "Simulation Paused" : "Simulation Resumed");
        }
    }

    void HandleResetInput()
    {
        if (resetAction != null && resetAction.WasPressedThisFrame())
        {
            if (menuManager && menuManager.cvCar) menuManager.cvCar.ResetSim();
            if (menuManager && menuManager.uaCar) menuManager.uaCar.ResetSim();

            // Reset graphs
            if (graphManager != null) graphManager.ResetGraphs();

            // Reset camera to starting position
            if (xrRig != null)
            {
                xrRig.position = cameraFixedPosition;
            }

            PauseManager.SetPaused(false);
            Debug.Log("Simulation Reset and Resumed. Camera and graphs reset.");
        }
    }

    void HandleMenuToggleInput()
    {
        if (toggleMenuAction != null && toggleMenuAction.WasPressedThisFrame())
        {
            if (menuCanvas != null && vrCamera != null && menuManager != null)
            {
                menuManager.ToggleMenu(vrCamera, distanceFromCamera);
            }
        }
    }

    public void SetFollowCar(Transform car)
    {
        followCar = car;
        Debug.Log($"Active car set to: {car.name}");
    }

    void HandleMovementWhenPaused()
    {
        if (!PauseManager.isSimulationPaused || xrRig == null) return;

        if (menuManager != null && menuManager.IsMenuOpen) return;

        if (moveWhenPausedAction != null)
        {
            Vector2 input = moveWhenPausedAction.ReadValue<Vector2>();
            if (input.magnitude > 0.1f)
            {
                Vector3 forward = vrCamera.forward;
                forward.y = 0;
                forward.Normalize();
                Vector3 right = vrCamera.right;
                right.y = 0;
                right.Normalize();
                Vector3 moveDirection = (forward * input.y + right * input.x).normalized;

                Vector3 movement = moveDirection * movementSpeed * Time.unscaledDeltaTime;
                xrRig.position += movement;
            }
        }
    }

    void HandleToggleGraphsInput()
    {
        if (toggleGraphsAction != null && toggleGraphsAction.WasPressedThisFrame())
        {
            if (graphManager != null)
            {
                graphManager.ToggleGraphs();
                Debug.Log($"Graphs toggled: {(graphManager.AreGraphsVisible() ? "Visible" : "Hidden")}");
            }
            else
            {
                Debug.LogWarning("[VRInputManager] Cannot toggle graphs - GraphManager reference is missing!");
            }
        }
    }
}