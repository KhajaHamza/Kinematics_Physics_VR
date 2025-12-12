using UnityEngine;

public class ConstantVelocityCar : MonoBehaviour
{
    public float velocity = 2f; // Constant velocity in m/s
    public float timeElapsed = 0f;
    public Transform tickSpawnPoint;
    private Vector3 startPosition;               // current run start
    private Vector3 originalStartPosition;       // the spawn position to reset back to

    // Public property to expose displacement from start position for graphing
    // Displacement = distance traveled from initial position at t=0
    public float Displacement
    {
        get
        {
            // Calculate displacement as the forward distance from the original start position
            return (transform.position - startPosition).magnitude;
        }
    }

    public Transform[] wheels;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;
    public float raycastDistance = 100f; // How far down to check for the road

    [Header("Simulation Control")]
    [Tooltip("Whether the car can move (always true for CV car, but time resets at trigger)")]
    public bool canMove = true;

    // Cache the Rigidbody component
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        originalStartPosition = startPosition;

        // Get and configure Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("ConstantVelocityCar: No Rigidbody found! Adding one...");
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Ensure proper kinematic settings
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement

        Debug.Log($"ConstantVelocityCar initialized - Rigidbody kinematic: {rb.isKinematic}");
    }

    void FixedUpdate()  // CHANGED FROM Update() to FixedUpdate()
    {
        if (PauseManager.isSimulationPaused) return;
        if (!canMove) return; // Don't move if disabled

        // Use Time.fixedDeltaTime for physics-based updates
        timeElapsed += Time.fixedDeltaTime;

        // Calculate target position
        float displacement = velocity * timeElapsed;
        Vector3 targetPosition = startPosition + Vector3.forward * displacement;

        // ✅ USE RIGIDBODY MOVEMENT FOR KINEMATIC OBJECTS
        if (rb != null)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            // Fallback (should never happen after Start())
            transform.position = targetPosition;
        }
    }

    void Update()  // Keep Update() for non-physics stuff
    {
        if (PauseManager.isSimulationPaused) return;
        if (!canMove) return;

        // Rotate wheels (visual only, can stay in Update)
        float wheelRotation = velocity * 50f * Time.deltaTime;
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(wheelRotation, 0, 0);
        }

        // Tick tape
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            if (tickMarkPrefab != null && tickSpawnPoint != null)
            {
                // Raycast down from the REAR SPAWN POINT
                RaycastHit hit;
                if (Physics.Raycast(tickSpawnPoint.position, Vector3.down, out hit, raycastDistance))
                {
                    Instantiate(tickMarkPrefab, hit.point, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("No surface found below car for tick mark placement");
                    Instantiate(tickMarkPrefab, tickSpawnPoint.position, Quaternion.identity);
                }
            }
            tickTimer = 0f;
        }
    }

    // Called by MenuManager to set new value and restart from current spot
    public void ApplyParamsAndRestart(float newVelocity)
    {
        velocity = newVelocity;

        // Restart run from wherever the car is now
        startPosition = transform.position;
        timeElapsed = 0f;
        tickTimer = 0f;

        Debug.Log($"ConstantVelocityCar: Applied new velocity {newVelocity} from position {startPosition}");
    }

    // Called when trigger fires - reset time but keep current position
    public void ResetTimeAtTrigger()
    {
        // Update start position to current position so car doesn't jump
        startPosition = transform.position;
        timeElapsed = 0f;
        tickTimer = 0f;

        Debug.Log($"ConstantVelocityCar: Time reset at trigger. New start position: {startPosition}");
    }

    // Called by VRControls (B) to reset back to original spawn
    public void ResetSim()
    {
        // Destroy all tick marks in the scene
        DestroyAllTickMarks();

        // Reset car position using Rigidbody
        if (rb != null)
        {
            rb.MovePosition(originalStartPosition);
        }
        else
        {
            transform.position = originalStartPosition;
        }

        startPosition = originalStartPosition;
        timeElapsed = 0f;
        tickTimer = 0f;
        canMove = true; // Re-enable movement

        Debug.Log("ConstantVelocityCar: Reset complete, tick marks cleared");
    }

    // Helper method to destroy ONLY instantiated tick mark clones (not the prefab)
    private void DestroyAllTickMarks()
    {
        if (tickMarkPrefab != null)
        {
            // Find all active GameObjects in the scene
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject obj in allObjects)
            {
                // Only destroy tick mark clones (not the original prefab)
                if (obj.name.Contains("(Clone)") &&
                    obj.name.Replace("(Clone)", "").Trim() == tickMarkPrefab.name)
                {
                    Destroy(obj);
                }
            }
        }
    }
}