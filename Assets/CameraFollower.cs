using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform targetCar;         // Drag your Accelerated Car here in the Inspector
    public Vector3 offset = new Vector3(0, 2, -5); // Adjust as needed
    public float smoothSpeed = 5f;

    private bool isFollowing = true;

    void LateUpdate()
    {
        // If simulation is paused, stop following
        if (PauseManager.isSimulationPaused)
        {
            isFollowing = false;
            return;
        }

        // If we're resuming from pause, snap the camera back to follow the car
        if (!isFollowing)
        {
            transform.position = targetCar.position + offset;
            isFollowing = true;
        }

        // Smooth follow
        if (targetCar != null && isFollowing)
        {
            Vector3 desiredPosition = targetCar.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(targetCar);
        }
    }
}
