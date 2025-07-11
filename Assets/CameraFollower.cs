using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target; // Accelerated car
    public Vector3 offset = new Vector3(0, 1.6f, -3f);

    void Update()
    {
        if (PauseManager.isSimulationPaused || target == null)
            return;

        transform.position = target.position + offset;
    }
}
