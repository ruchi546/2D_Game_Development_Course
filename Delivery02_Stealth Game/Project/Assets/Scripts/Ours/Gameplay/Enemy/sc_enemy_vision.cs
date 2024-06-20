using UnityEngine;

public class sc_EnemyVisionJ : MonoBehaviour
{
    [SerializeField] float detectionRange;
    [SerializeField] float visionAngle;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] LayerMask whatIsVisible;

    [SerializeField] private AudioClip alarmSound;
    private bool soundPlayed = false;

    private GameObject alarmObject;

    private void Start()
    {
        alarmObject = transform.GetChild(1).gameObject;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        var direction = Quaternion.AngleAxis(visionAngle / 2, transform.forward) * transform.right;

        Gizmos.DrawRay(transform.position, direction * detectionRange);

        var direction2 = Quaternion.AngleAxis(-visionAngle / 2, transform.forward) * transform.right;

        Gizmos.DrawRay(transform.position, direction2 * detectionRange);
        Gizmos.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (DetectPlayer())
        {
            alarmObject.SetActive(true);

            if (!soundPlayed)
            {
                SoundManager.Instance.PlaySound(alarmSound);
                soundPlayed = true;
                SceneManager.Instance.LoadSceneWithFade(1);
            }
        }
        else
        {
            alarmObject.SetActive(false);
            soundPlayed = false;
        }
       
    }

    private Transform DetectPlayer()
    {
        Transform player = null;
        if (PlayerInRange(ref player))
        {
            if (PlayerInAngle(ref player))
            {
                PlayerIsVisible(ref player);
            }
        }

        return player;
    }

    private bool PlayerInRange(ref Transform player)
    {
        Collider2D[] playerColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, whatIsPlayer);

        if (playerColliders.Length == 0)
        {
            return false;
        }

        foreach (var item in playerColliders)
        {
            player = item.transform;
        }

        return true;
    }

    private bool PlayerInAngle(ref Transform player)
    {
        if (player != null)
        {
            if (GetAngle(player) > visionAngle / 2)
            {
                player = null;
            }
        }
        return player != null;
    }

    private float GetAngle(Transform player)
    {
        Vector2 playerDir = player.position - transform.position;
        float angle = Vector2.Angle(playerDir, transform.right);

        return angle;
    }

    private bool PlayerIsVisible(ref Transform player)
    {

        if (player != null)
        {
            var isVisible = IsVisible(player);

            if (!isVisible) 
            {

                player = null;

            }

        }

        return player != null;
    }

    private bool IsVisible(Transform player)
    {
        Vector3 direction = player.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRange, whatIsVisible);

        return (hit.collider.transform == player);
    }
}