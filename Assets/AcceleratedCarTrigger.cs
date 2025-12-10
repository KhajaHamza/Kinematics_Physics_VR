using UnityEngine;

public class AcceleratedCarTrigger : MonoBehaviour
{
    public AcceleratedCarMovementWithTicks carMovement;
    public GraphManager graphManager;
    public ConstantVelocityCar cvCar;

    [Header("Optional Distance Check (in addition to trigger)")]
    public bool useDistanceCheck = false;
    [Tooltip("Meters between CV front and UA rear to trigger start")]
    public float triggerDistance = 0.1f;
    public Transform cvFrontBumper;   // point at the front of the CV car
    public Transform uaRearWheel;     // point at the back tire of the accelerated car

    private bool hasTriggered = false;

    void Start()
    {
        // Ensure this GameObject is active
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"AcceleratedCarTrigger on {gameObject.name} is inactive! Enabling...");
            gameObject.SetActive(true);
        }

        // Verify collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"AcceleratedCarTrigger on {gameObject.name} has no Collider component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"AcceleratedCarTrigger on {gameObject.name} Collider is not set as Trigger!");
        }

        // Verify references
        if (carMovement == null)
        {
            Debug.LogError($"AcceleratedCarTrigger on {gameObject.name} has no carMovement reference!");
        }
    }

    void Update()
    {
        // When distance mode is enabled, also allow a physics-free trigger.
        if (hasTriggered || !useDistanceCheck) return;
        if (cvFrontBumper == null || uaRearWheel == null) return;

        float distance = Vector3.Distance(cvFrontBumper.position, uaRearWheel.position);
        if (distance <= triggerDistance)
        {
            TriggerAcceleratedCar();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[AcceleratedCarTrigger] OnTriggerEnter called! Collider: {other.name}, Tag: {other.tag}, HasTriggered: {hasTriggered}");

        if (other.CompareTag("ConstantCar"))
        {
            Debug.Log($"[AcceleratedCarTrigger] Tag match confirmed! Triggering accelerated car...");
            TriggerAcceleratedCar();
        }
        else
        {
            Debug.LogWarning($"[AcceleratedCarTrigger] Tag mismatch! Expected 'ConstantCar', got '{other.tag}'");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Fallback: if OnTriggerEnter somehow missed it, catch it here
        if (!hasTriggered && other.CompareTag("ConstantCar"))
        {
            Debug.Log($"[AcceleratedCarTrigger] OnTriggerStay fallback triggered!");
            TriggerAcceleratedCar();
        }
    }

    private void TriggerAcceleratedCar()
    {
        if (hasTriggered)
        {
            Debug.Log("[AcceleratedCarTrigger] Already triggered, ignoring.");
            return;
        }

        hasTriggered = true;
        Debug.Log("[AcceleratedCarTrigger] ===== TRIGGER FIRED ===== Starting accelerated car movement!");

        // Keep CV car time aligned with the moment of contact for graphing
        if (cvCar != null)
        {
            cvCar.ResetTimeAtTrigger();
            Debug.Log("[AcceleratedCarTrigger] CV car time reset at trigger.");
        }
        else
        {
            Debug.LogWarning("[AcceleratedCarTrigger] cvCar reference is null!");
        }

        if (carMovement != null)
        {
            carMovement.StartMoving();
            Debug.Log($"[AcceleratedCarTrigger] Called carMovement.StartMoving(). IsMoving should now be: {carMovement.IsMoving}");
        }
        else
        {
            Debug.LogError("[AcceleratedCarTrigger] carMovement reference is null! Cannot start car!");
        }

        // If graphs are already running (started from UI), don't clear data.
        if (graphManager != null)
        {
            graphManager.StartSimulation(clearExistingData: false);
            Debug.Log("[AcceleratedCarTrigger] Graph simulation resumed.");
        }
        else
        {
            Debug.LogWarning("[AcceleratedCarTrigger] graphManager reference is null!");
        }
    }

    // Public method to reset the trigger (called when simulation resets)
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("[AcceleratedCarTrigger] Trigger reset - ready to fire again.");
    }
}
