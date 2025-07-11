using UnityEngine;

public class ConstantVelocityCar : MonoBehaviour
{
    public float velocity = 2f; // Constant velocity in m/s
    private float timeElapsed = 0f;
    private Vector3 startPosition;
    public Transform tickSpawnPoint;

    public Transform[] wheels;

    [Header("Tick Tape Settings")]
    public GameObject tickMarkPrefab;
    public float tickInterval = 0.5f;
    private float tickTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }
    void Update()
    {
        if (PauseManager.isSimulationPaused)
            return;

        timeElapsed += Time.deltaTime;

        float displacement = velocity * timeElapsed;
        Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        transform.position = startPosition + flatForward * displacement;


        float wheelRotation = velocity * 50f * Time.deltaTime;
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(wheelRotation, 0, 0);
        }

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            if (tickSpawnPoint != null)
            {
                Instantiate(tickMarkPrefab, tickSpawnPoint.position, Quaternion.identity);
            }

            tickTimer = 0f;
        }
    }
}