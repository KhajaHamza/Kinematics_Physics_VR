using UnityEngine;

public class FollowMidpoint : MonoBehaviour
{
    public Transform car1;
    public Transform car2;
    public float smoothSpeed = 2f;

    void LateUpdate()
    {
        if (car1 == null || car2 == null) return;

        Vector3 midpoint = (car1.position + car2.position) / 2f;
        Vector3 desiredPosition = new Vector3(midpoint.x, transform.position.y, midpoint.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
