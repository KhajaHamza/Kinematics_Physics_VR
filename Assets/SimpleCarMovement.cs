using UnityEngine;

public class AcceleratedCarMovementWithTicks : MonoBehaviour
{
    public float acceleration = 1f;
    public float initialVelocity = 0f;
    private float timeElapsed = 0f;
    private Vector3 startPosition;

    public Transform[] wheels;
    private float currentVelocity;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        currentVelocity = initialVelocity;
    }

    void Update()
    {
        if (PauseManager.isSimulationPaused)
            return;

        timeElapsed += Time.deltaTime;

        float displacement = initialVelocity * timeElapsed + 0.5f * acceleration * timeElapsed * timeElapsed;
        transform.position = startPosition + Vector3.forward * displacement;

        currentVelocity = initialVelocity + acceleration * timeElapsed;

        float wheelRotation = currentVelocity * 50f * Time.deltaTime;
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(wheelRotation, 0, 0);
        }

        // Handle tick tape spawning
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            Instantiate(tickMarkPrefab, transform.position, Quaternion.identity);
            tickTimer = 0f;
        }
    }
}