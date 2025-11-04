using UnityEngine;

public class AcceleratedCarMovementWithTicks : MonoBehaviour
{
    public float acceleration = 1f;
    public float initialVelocity = 0f;

    public float timeElapsed = 0f;
    private Vector3 startPosition;               // current run start
    private Vector3 originalStartPosition;       // original spawn position

    public Transform[] wheels;
    private float currentVelocity;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;
    public float raycastDistance = 100f; // How far down to check for the road

    void Start()
    {
        startPosition = transform.position;
        originalStartPosition = startPosition;
        currentVelocity = initialVelocity;
    }

    void Update()
    {
        if (PauseManager.isSimulationPaused) return;

        timeElapsed += Time.deltaTime;

        float displacement = initialVelocity * timeElapsed + 0.5f * acceleration * timeElapsed * timeElapsed;
        transform.position = startPosition + Vector3.forward * displacement;

        currentVelocity = initialVelocity + acceleration * timeElapsed;

        float wheelRotation = currentVelocity * 50f * Time.deltaTime;
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(wheelRotation, 0, 0);
        }

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            if (tickMarkPrefab != null)
            {
                // Raycast down to find the road surface
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
                {
                    // Spawn tick mark on the surface below
                    Instantiate(tickMarkPrefab, hit.point, Quaternion.identity);
                }
                else
                {
                    // Fallback: spawn at car position if no surface found
                    Debug.LogWarning("No surface found below car for tick mark placement");
                    Instantiate(tickMarkPrefab, transform.position, Quaternion.identity);
                }
            }

            tickTimer = 0f;
        }
    }

    // Called by MenuManager to set values and restart from current spot
    public void ApplyParamsAndRestart(float newU, float newA)
    {
        initialVelocity = newU;
        acceleration = newA;

        startPosition = transform.position;   // restart run from here
        timeElapsed = 0f;
        currentVelocity = initialVelocity;
        tickTimer = 0f;
    }

    // FIXED: Called by VRControls (B) to reset back to original spawn
    // Now also destroys all tick marks
    public void ResetSim()
    {
        // Destroy all tick marks in the scene
        DestroyAllTickMarks();

        // Reset car position and physics
        transform.position = originalStartPosition;
        startPosition = originalStartPosition;
        timeElapsed = 0f;
        currentVelocity = initialVelocity;
        tickTimer = 0f;

        Debug.Log("AcceleratedCar: Reset complete, tick marks cleared");
    }

    // FIXED: Helper method to destroy ONLY instantiated tick mark clones (not the prefab)
    private void DestroyAllTickMarks()
    {
        if (tickMarkPrefab != null)
        {
            // Find all instances of the tick mark prefab by name
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // CRITICAL FIX: Only destroy objects that have "(Clone)" in their name
                // This ensures we don't destroy the original prefab
                if (obj.name.Contains("(Clone)") &&
                    obj.name.Replace("(Clone)", "").Trim() == tickMarkPrefab.name)
                {
                    Destroy(obj);
                }
            }
        }
    }
}