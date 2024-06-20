using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 5f;

    private Patrolling patrolling;

    void Start()
    {
        patrolling = GetComponent<Patrolling>();
    }

    private void FixedUpdate()
    {
        if (patrolling == null)
            return;

        MoveTowardsWaypoint();
        RotateSprite();
    }

    private void MoveTowardsWaypoint()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, patrolling.targetPosition, step);
    }

    private void RotateSprite()
    {
        Vector3 direction = patrolling.targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        this.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}