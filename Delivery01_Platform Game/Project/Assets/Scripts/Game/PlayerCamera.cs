using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 offsetDistance = new Vector3(0f, 0f, -10f);
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;

    void Update()
    {
        if (playerTransform != null) 
        {
            targetPosition = playerTransform.position + offsetDistance;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
