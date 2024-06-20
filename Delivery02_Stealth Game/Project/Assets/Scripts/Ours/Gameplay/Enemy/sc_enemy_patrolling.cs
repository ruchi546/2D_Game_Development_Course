using UnityEngine;

public class Patrolling : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public Vector3 targetPosition;
    public float movementThisFrame;

    private EnemyMovement EnemyMovement;

    private void Start()
    {
        EnemyMovement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (waypoints.Length == 0)
        {
            return;
        }

        targetPosition = waypoints[currentWaypointIndex].position;
        movementThisFrame = EnemyMovement.speed * Time.deltaTime;

        if (transform.position == targetPosition)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

}
