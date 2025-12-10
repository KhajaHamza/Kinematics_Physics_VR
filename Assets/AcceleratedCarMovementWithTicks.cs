using UnityEngine;

public class AcceleratedCarMovementWithTicks : MonoBehaviour
{
    public enum SimulationMode { SingleCar, TwoCars }
    public SimulationMode simMode = SimulationMode.TwoCars;

    public float acceleration = 1f;
    public float initialVelocity = 0f;

    public float timeElapsed = 0f;
    private Vector3 startPosition;
    private Vector3 originalStartPosition;

    // Public property to expose displacement from start position for graphing
    // Displacement = distance traveled from initial position at t=0
    public float Displacement
    {
        get
        {
            // Calculate displacement as the forward distance from the start position
            return (transform.position - startPosition).magnitude;
        }
    }

    public Transform[] wheels;
    private float currentVelocity;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;
    public float raycastDistance = 100f;

    private bool isMoving = false;
    private GameObject triggerObj;
    private AcceleratedCarTrigger triggerComponent;

    // Expose read-only flag so other systems (graphs, triggers) can check
    // whether the accelerated car is currently moving without risking null refs.
    public bool IsMoving => isMoving;

    void Start()
    {
        startPosition = transform.position;
        originalStartPosition = startPosition;
        currentVelocity = initialVelocity;

        // Find trigger GameObject by name
        triggerObj = transform.Find("StartTrigger")?.gameObject;

        // Also try to find the trigger component directly (might be on this GameObject or any child)
        if (triggerComponent == null)
        {
            triggerComponent = GetComponentInChildren<AcceleratedCarTrigger>();
        }

        // If still not found, try finding by component on this GameObject
        if (triggerComponent == null)
        {
            triggerComponent = GetComponent<AcceleratedCarTrigger>();
        }

        if (simMode == SimulationMode.SingleCar)
        {
            isMoving = true;
            if (triggerObj) triggerObj.SetActive(false);
        }
        else
        {
            isMoving = false;
            if (triggerObj) triggerObj.SetActive(true);
            if (triggerComponent == null)
            {
                Debug.LogWarning($"[AcceleratedCarMovementWithTicks] No AcceleratedCarTrigger component found on {gameObject.name} or its children! Trigger will not work.");
            }
        }
    }

    void Update()
    {
        if (!isMoving) return;
        if (PauseManager.isSimulationPaused) return;

        timeElapsed += Time.deltaTime;

        float displacement = initialVelocity * timeElapsed
                             + 0.5f * acceleration * timeElapsed * timeElapsed;

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
            TickPlacement();
            tickTimer = 0f;
        }
    }

    void TickPlacement()
    {
        if (tickMarkPrefab == null) return;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            Instantiate(tickMarkPrefab, hit.point, Quaternion.identity);
        }
        else
        {
            Instantiate(tickMarkPrefab, transform.position, Quaternion.identity);
        }
    }

    public void StartMoving()
    {
        isMoving = true;
        startPosition = transform.position;
        timeElapsed = 0f;
        currentVelocity = initialVelocity;
        tickTimer = 0f;
    }

    public void ApplyParamsAndRestart(float newU, float newA)
    {
        initialVelocity = newU;
        acceleration = newA;

        startPosition = transform.position;
        timeElapsed = 0f;
        currentVelocity = initialVelocity;
        tickTimer = 0f;

        // Reset the trigger component if it exists (for multiple car mode)
        if (simMode == SimulationMode.TwoCars)
        {
            if (triggerComponent != null)
            {
                triggerComponent.ResetTrigger();
            }
            else if (triggerObj != null)
            {
                // Fallback: try to get component from triggerObj
                AcceleratedCarTrigger trigger = triggerObj.GetComponent<AcceleratedCarTrigger>();
                if (trigger != null)
                {
                    trigger.ResetTrigger();
                    triggerComponent = trigger; // Cache it for next time
                }
            }
        }

        if (simMode == SimulationMode.SingleCar)
        {
            isMoving = true;
            if (triggerObj) triggerObj.SetActive(false);
        }
        else
        {
            isMoving = false;
            if (triggerObj) triggerObj.SetActive(true);
        }
    }

    public void ResetSim()
    {
        DestroyAllTickMarks();

        transform.position = originalStartPosition;
        startPosition = originalStartPosition;
        timeElapsed = 0f;
        currentVelocity = initialVelocity;
        tickTimer = 0f;

        // Reset the trigger component if it exists
        if (triggerComponent != null)
        {
            triggerComponent.ResetTrigger();
        }
        else if (triggerObj != null)
        {
            // Fallback: try to get component from triggerObj
            AcceleratedCarTrigger trigger = triggerObj.GetComponent<AcceleratedCarTrigger>();
            if (trigger != null)
            {
                trigger.ResetTrigger();
                triggerComponent = trigger; // Cache it for next time
            }
        }

        if (simMode == SimulationMode.SingleCar)
        {
            isMoving = true;
            if (triggerObj) triggerObj.SetActive(false);
        }
        else
        {
            isMoving = false;
            if (triggerObj) triggerObj.SetActive(true);
        }
    }

    private void DestroyAllTickMarks()
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("(Clone)") &&
                obj.name.Replace("(Clone)", "").Trim() == tickMarkPrefab.name)
            {
                Destroy(obj);
            }
        }
    }
}