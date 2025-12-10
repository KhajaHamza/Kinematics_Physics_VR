using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelRoot;
    [SerializeField] GameObject panelSingle;
    [SerializeField] GameObject panelMultiple;

    public bool IsMenuOpen
    {
        get { return panelRoot != null && panelRoot.activeSelf; }
    }

    [Header("Single Car UI")]
    [SerializeField] Toggle toggleCV_Single;
    [SerializeField] Toggle toggleUA_Single;
    [SerializeField] TMP_InputField inputU_Single;
    [SerializeField] TMP_InputField inputA_Single;
    [SerializeField] CanvasGroup accelRow_Single;
    [SerializeField] Button btnApply_Single;

    [Header("Multiple Car UI")]
    [SerializeField] TMP_InputField inputU_CV;
    [SerializeField] TMP_InputField inputU_UA;
    [SerializeField] TMP_InputField inputA_UA;
    [SerializeField] Button btnApply_Multiple;

    [Header("Navigation Buttons")]
    [SerializeField] Button btnShowSingle;
    [SerializeField] Button btnShowMultiple;

    [Header("Graph Control")]
    [Tooltip("Toggle for Single Car panel - checked = graphs shown, unchecked = graphs hidden")]
    [SerializeField] Toggle toggleGraphs_Single;
    [Tooltip("Toggle for Multiple Car panel - checked = graphs shown, unchecked = graphs hidden")]
    [SerializeField] Toggle toggleGraphs_Multiple;

    [Header("Car Refs")]
    public ConstantVelocityCar cvCar;
    public AcceleratedCarMovementWithTicks uaCar;

    [Header("External Managers")]
    [SerializeField] VRInputManager vrInputManager;
    [SerializeField] GraphManager graphManager;


    // Cache the CANVAS transform's world position and rotation
    private Vector3 canvasWorldPosition;
    private Quaternion canvasWorldRotation;
    private bool menuPositionCached = false;

    // Reference to the actual Canvas GameObject (parent of all panels)
    private Transform canvasTransform;

    void Awake()
    {
        // Get the canvas transform (this script should be on the Canvas)
        canvasTransform = transform;

        // Single panel wiring
        toggleCV_Single.onValueChanged.AddListener(_ => UpdateSingleAccelRow());
        toggleUA_Single.onValueChanged.AddListener(_ => UpdateSingleAccelRow());
        btnApply_Single.onClick.AddListener(ApplySingle);

        // Multiple panel wiring
        btnApply_Multiple.onClick.AddListener(ApplyMultiple);

        // Navigation panel wiring
        if (btnShowSingle) btnShowSingle.onClick.AddListener(ShowSingle);
        if (btnShowMultiple) btnShowMultiple.onClick.AddListener(ShowMultiple);

        // Graph toggle wiring removed - graphs are now controlled via XR controller (Y button)
        // Toggles are kept for UI state display only (updated via UpdateGraphToggleStates)

        // Initial state update
        UpdateSingleAccelRow();

        // Ensure all panels are hidden at start
        if (panelRoot) panelRoot.SetActive(false);
        if (panelSingle) panelSingle.SetActive(false);
        if (panelMultiple) panelMultiple.SetActive(false);
    }

    // ===== Public Toggle Method (called by VRInputManager) =====
    public void ToggleMenu(Transform vrCamera, float distanceFromCamera)
    {
        bool show = !IsMenuOpen;

        if (show)
        {
            // Position the ENTIRE CANVAS in front of the camera
            PositionMenuInFrontOfCamera(vrCamera, distanceFromCamera);

            // Cache this position for when we switch between panels
            canvasWorldPosition = canvasTransform.position;
            canvasWorldRotation = canvasTransform.rotation;
            menuPositionCached = true;

            // Show the root panel initially
            ShowRoot();

            // Update graph toggle states to reflect current visibility
            UpdateGraphToggleStates();

            PauseManager.SetPaused(true);
        }
        else
        {
            // Hide all panels
            if (panelRoot) panelRoot.SetActive(false);
            if (panelSingle) panelSingle.SetActive(false);
            if (panelMultiple) panelMultiple.SetActive(false);

            PauseManager.SetPaused(false);
            menuPositionCached = false;
        }
    }

    void PositionMenuInFrontOfCamera(Transform vrCamera, float distanceFromCamera)
    {
        Vector3 cameraForward = vrCamera.forward;
        cameraForward.y = 0; // Keep menu level with horizon
        cameraForward.Normalize();

        Vector3 cameraPosition = vrCamera.position;
        Vector3 targetPosition = cameraPosition + cameraForward * distanceFromCamera;

        // Position the canvas
        canvasTransform.position = targetPosition;

        // Make canvas face the camera (but keep it upright)
        Vector3 lookDirection = cameraPosition - canvasTransform.position;
        lookDirection.y = 0;
        canvasTransform.rotation = Quaternion.LookRotation(-lookDirection);
    }

    // ===== Panel Navigation =====
    public void ShowRoot()
    {
        if (panelRoot) panelRoot.SetActive(true);
        if (panelSingle) panelSingle.SetActive(false);
        if (panelMultiple) panelMultiple.SetActive(false);

        // Keep canvas at cached position
        if (menuPositionCached)
        {
            canvasTransform.position = canvasWorldPosition;
            canvasTransform.rotation = canvasWorldRotation;
        }
    }

    public void ShowSingle()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (panelSingle) panelSingle.SetActive(true);
        if (panelMultiple) panelMultiple.SetActive(false);

        // Keep canvas at cached position
        if (menuPositionCached)
        {
            canvasTransform.position = canvasWorldPosition;
            canvasTransform.rotation = canvasWorldRotation;
        }

        UpdateSingleAccelRow();

        // Update graph toggle state when showing single panel
        UpdateGraphToggleStates();
    }

    public void ShowMultiple()
    {
        if (uaCar != null)
            uaCar.simMode = AcceleratedCarMovementWithTicks.SimulationMode.TwoCars;

        if (panelRoot) panelRoot.SetActive(false);
        if (panelSingle) panelSingle.SetActive(false);
        if (panelMultiple) panelMultiple.SetActive(true);

        // Keep canvas at cached position
        if (menuPositionCached)
        {
            canvasTransform.position = canvasWorldPosition;
            canvasTransform.rotation = canvasWorldRotation;
        }

        // Enable both cars for Multiple Car Mode
        if (cvCar) cvCar.gameObject.SetActive(true);
        if (uaCar) uaCar.gameObject.SetActive(true);

        // Set follow to UA car in multiple mode
        if (vrInputManager) vrInputManager.SetFollowCar(uaCar.transform);

        // Update graph toggle state when showing multiple panel
        UpdateGraphToggleStates();
    }

    // ===== Single panel behavior =====
    void UpdateSingleAccelRow()
    {
        bool needA = toggleUA_Single.isOn;
        if (uaCar != null)
        {
            uaCar.simMode = toggleUA_Single.isOn ?
                AcceleratedCarMovementWithTicks.SimulationMode.SingleCar :
                AcceleratedCarMovementWithTicks.SimulationMode.TwoCars;
        }

        // UI Logic
        if (accelRow_Single)
        {
            accelRow_Single.alpha = needA ? 1f : 0.35f;
            accelRow_Single.interactable = needA;
            accelRow_Single.blocksRaycasts = needA;
        }

        // Scene Logic
        if (cvCar && uaCar)
        {
            if (toggleCV_Single.isOn)
            {
                cvCar.gameObject.SetActive(true);
                uaCar.gameObject.SetActive(false);
            }
            else if (toggleUA_Single.isOn)
            {
                cvCar.gameObject.SetActive(false);
                uaCar.gameObject.SetActive(true);
            }
        }

        // Set follow target
        if (vrInputManager && cvCar && uaCar)
        {
            Transform followTarget = toggleCV_Single.isOn ? cvCar.transform : uaCar.transform;
            vrInputManager.SetFollowCar(followTarget);
        }
    }

    // ===== APPLY ACTIONS =====
    void ApplySingle()
    {
        float u = Parse(inputU_Single, 0f);
        float a = Parse(inputA_Single, 0f);

        // CRITICAL: Clear graph data BEFORE starting cars
        // This ensures graphs start collecting from true t=0 when simulation unpauses
        if (graphManager != null)
        {
            graphManager.StartSimulation(clearExistingData: true);
        }

        if (toggleCV_Single.isOn)
        {
            if (cvCar && cvCar.gameObject.activeSelf)
            {
                cvCar.ApplyParamsAndRestart(u);
                Debug.Log($"[Single] Applied: CV v={u}");
            }
        }
        else
        {
            if (uaCar && uaCar.gameObject.activeSelf)
            {
                uaCar.ApplyParamsAndRestart(u, a);
                Debug.Log($"[Single] Applied: UA u={u} a={a}");
            }
        }

        // Hide menu and unpause - NOW graphs will start sampling from t=0
        if (panelRoot) panelRoot.SetActive(false);
        if (panelSingle) panelSingle.SetActive(false);
        if (panelMultiple) panelMultiple.SetActive(false);

        PauseManager.SetPaused(false);
        menuPositionCached = false;
    }

    void ApplyMultiple()
    {
        float uCV = Parse(inputU_CV, 2f);
        float uUA = Parse(inputU_UA, 0f);
        float aUA = Parse(inputA_UA, 1f);

        // CRITICAL: Clear graph data BEFORE resetting cars
        // This ensures graphs start collecting from true t=0 when simulation unpauses
        if (graphManager != null)
        {
            graphManager.StartSimulation(clearExistingData: true);
        }

        // Reset both cars to their original positions
        // This ensures they start from their manually positioned locations
        if (cvCar)
        {
            cvCar.ResetSim();
            cvCar.ApplyParamsAndRestart(uCV);
        }
        if (uaCar)
        {
            uaCar.ResetSim();
            uaCar.ApplyParamsAndRestart(uUA, aUA);
        }

        Debug.Log($"[Multiple] Applied: CV u={uCV}, UA u={uUA} a={aUA}");

        // Hide menu and unpause - NOW graphs will start sampling from t=0
        if (panelRoot) panelRoot.SetActive(false);
        if (panelSingle) panelSingle.SetActive(false);
        if (panelMultiple) panelMultiple.SetActive(false);

        PauseManager.SetPaused(false);
        menuPositionCached = false;
    }

    float Parse(TMP_InputField f, float def)
        => (f && float.TryParse(f.text, out var v)) ? v : def;

    // ===== Graph Control =====
    // Note: Graph toggling is now handled via XR controller (Y button) in VRInputManager
    // This method is kept for UI state synchronization only (to reflect controller state in UI)
    void UpdateGraphToggleStates()
    {
        if (graphManager != null)
        {
            bool graphsVisible = graphManager.AreGraphsVisible();

            // Update Single Car panel toggle (without triggering event)
            if (toggleGraphs_Single != null)
            {
                toggleGraphs_Single.SetIsOnWithoutNotify(graphsVisible);
            }

            // Update Multiple Car panel toggle (without triggering event)
            if (toggleGraphs_Multiple != null)
            {
                toggleGraphs_Multiple.SetIsOnWithoutNotify(graphsVisible);
            }
        }
    }

}