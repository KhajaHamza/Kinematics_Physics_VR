using UnityEngine;

public class ConstantVelocityCar : MonoBehaviour
{
    public float velocity = 2f; // Constant velocity in m/s
    public float timeElapsed = 0f;

    private Vector3 startPosition;               // current run start
    private Vector3 originalStartPosition;       // the spawn position to reset back to

    public Transform tickSpawnPoint;
    public Transform[] wheels;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;
    public float raycastDistance = 100f; // How far down to check for the road

    void Start()
    {
        startPosition = transform.position;
        originalStartPosition = startPosition;
    }

    void Update()
    {
        if (PauseManager.isSimulationPaused) return;

        timeElapsed += Time.deltaTime;

        // Move in forward direction (Z-axis)
        float displacement = velocity * timeElapsed;
        transform.position = startPosition + Vector3.forward * displacement;

        // Rotate wheels
        float wheelRotation = velocity * 50f * Time.deltaTime;
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(wheelRotation, 0, 0);
        }

        // Tick tape
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            if (tickMarkPrefab != null)
            {
                Vector3 spawnPos = tickSpawnPoint != null ? tickSpawnPoint.position : transform.position;

                // Raycast down to find the road surface
                RaycastHit hit;
                if (Physics.Raycast(spawnPos, Vector3.down, out hit, raycastDistance))
                {
                    // Spawn tick mark on the surface below
                    Instantiate(tickMarkPrefab, hit.point, Quaternion.identity);
                }
                else
                {
                    // Fallback: spawn at original position if no surface found
                    Debug.LogWarning("No surface found below car for tick mark placement");
                    Instantiate(tickMarkPrefab, spawnPos, Quaternion.identity);
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
    }

    // FIXED: Called by VRControls (B) to reset back to original spawn
    // Now also destroys all tick marks
    public void ResetSim()
    {
        // Destroy all tick marks in the scene
        DestroyAllTickMarks();

        // Reset car position
        transform.position = originalStartPosition;
        startPosition = originalStartPosition;
        timeElapsed = 0f;
        tickTimer = 0f;

        Debug.Log("ConstantVelocityCar: Reset complete, tick marks cleared");
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