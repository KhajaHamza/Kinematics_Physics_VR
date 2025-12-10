using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the graph canvas centered in front of the VR camera at a fixed offset.
/// Designed for a World Space canvas that should hover in view during simulation.
/// 
/// CRITICAL DESIGN NOTE - Background Data Collection:
/// When graphs are "hidden", we move the canvas far away (y=-1000, z=-1000) instead 
/// of calling SetActive(false). This is because XCharts CANNOT add data to inactive 
/// GameObjects. By keeping the canvas active but out of view, charts can continuously 
/// collect data in the background, so when the user shows graphs at any time (e.g., t=7s), 
/// they see the complete history from t=0 to t=7s.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class GraphCanvasFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform vrCamera;

    [Header("Placement")]
    [Tooltip("Distance from camera in meters (forward direction). Recommended: 3-4m for comfortable viewing.")]
    [SerializeField] private float distanceFromCamera = 3.5f;
    [Tooltip("Vertical angle offset in degrees (negative = below horizon, positive = above). Recommended: -10 to -15 for comfortable viewing.")]
    [SerializeField] private float verticalAngleOffset = -12f;
    [Tooltip("Height offset from camera eye level in meters (positive = above, negative = below). Use this for fine-tuning.")]
    [SerializeField] private float heightOffset = -0.3f;
    [SerializeField] private float positionLerp = 15f;
    [SerializeField] private float rotationLerp = 15f;
    [SerializeField] private bool yawOnly = true;

    private Canvas canvas;
    private bool isActive = false;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    void Start()
    {
        // Auto-find VR camera if not assigned
        if (vrCamera == null)
        {
            FindVRCamera();
        }
    }

    void FindVRCamera()
    {
        // Try to find the main camera (usually the VR camera)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            vrCamera = mainCam.transform;
            Debug.Log($"[GraphCanvasFollower] Auto-found VR camera: {vrCamera.name}");
            return;
        }

        // Try to find by tag
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
        {
            vrCamera = camObj.transform;
            Debug.Log($"[GraphCanvasFollower] Auto-found VR camera by tag: {vrCamera.name}");
            return;
        }

        // Try to find any camera in the scene (fallback)
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        if (allCameras.Length > 0)
        {
            // Prefer cameras that are active and enabled
            foreach (Camera cam in allCameras)
            {
                if (cam.gameObject.activeInHierarchy && cam.enabled)
                {
                    vrCamera = cam.transform;
                    Debug.Log($"[GraphCanvasFollower] Auto-found camera: {vrCamera.name}");
                    return;
                }
            }
            // If no active camera found, use the first one
            vrCamera = allCameras[0].transform;
            Debug.Log($"[GraphCanvasFollower] Auto-found camera (fallback): {vrCamera.name}");
            return;
        }

        Debug.LogWarning("[GraphCanvasFollower] Could not auto-find VR camera! Please assign it in the Inspector.");
    }

    void LateUpdate()
    {
        // Only update position when canvas is active
        if (!isActive || vrCamera == null) return;

        // Calculate target position with comfortable viewing angle
        Vector3 cameraForward = vrCamera.forward;
        Vector3 cameraPosition = vrCamera.position;

        // Create a direction that's angled down for comfortable viewing
        // First, get horizontal forward direction
        Vector3 horizontalForward = cameraForward;
        horizontalForward.y = 0;
        horizontalForward.Normalize();

        // Apply vertical angle offset (negative = look down)
        float angleRad = verticalAngleOffset * Mathf.Deg2Rad;
        Vector3 angledDirection = horizontalForward * Mathf.Cos(angleRad) + Vector3.down * Mathf.Sin(-angleRad);
        angledDirection.Normalize();

        // Position at specified distance along the angled direction
        Vector3 targetPos = cameraPosition + angledDirection * distanceFromCamera;

        // Apply additional height offset for fine-tuning
        targetPos.y += heightOffset;

        // Keep the canvas facing the camera (but keep it upright)
        Quaternion facingRotation;
        if (yawOnly)
        {
            // Only rotate around Y axis to keep canvas upright
            Vector3 directionToCamera = (cameraPosition - targetPos);
            directionToCamera.y = 0; // Keep horizontal
            facingRotation = Quaternion.LookRotation(-directionToCamera);
        }
        else
        {
            // Face camera directly
            Vector3 directionToCamera = (cameraPosition - targetPos);
            facingRotation = Quaternion.LookRotation(-directionToCamera);
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * positionLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, facingRotation, Time.deltaTime * rotationLerp);
    }

    /// <summary>
    /// Called when graphs should be shown - positions canvas in front of camera immediately
    /// </summary>
    public void ShowGraphs()
    {
        isActive = true;

        // Ensure canvas is active (in case it was deactivated elsewhere)
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Immediately position in front of camera with comfortable viewing angle (no lerp delay)
        if (vrCamera != null)
        {
            Vector3 cameraForward = vrCamera.forward;
            Vector3 cameraPosition = vrCamera.position;

            // Create a direction that's angled down for comfortable viewing
            Vector3 horizontalForward = cameraForward;
            horizontalForward.y = 0;
            horizontalForward.Normalize();

            // Apply vertical angle offset (negative = look down)
            float angleRad = verticalAngleOffset * Mathf.Deg2Rad;
            Vector3 angledDirection = horizontalForward * Mathf.Cos(angleRad) + Vector3.down * Mathf.Sin(-angleRad);
            angledDirection.Normalize();

            // Position at specified distance along the angled direction
            Vector3 targetPos = cameraPosition + angledDirection * distanceFromCamera;
            targetPos.y += heightOffset;

            transform.position = targetPos;

            // Face camera
            Vector3 directionToCamera = (cameraPosition - targetPos);
            if (yawOnly)
            {
                directionToCamera.y = 0;
            }
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }

    /// <summary>
    /// Called when graphs should be hidden
    /// CRITICAL: We move the canvas far away instead of deactivating it,
    /// because XCharts cannot add data to inactive GameObjects.
    /// This allows continuous background data collection even when graphs are hidden.
    /// </summary>
    public void HideGraphs()
    {
        isActive = false;

        // Move canvas far away (1000 units below and behind) instead of deactivating
        // This keeps the GameObject active so XCharts can continue adding data
        transform.position = new Vector3(0, -1000f, -1000f);

        // DO NOT call gameObject.SetActive(false) - it would prevent data collection!
    }
}