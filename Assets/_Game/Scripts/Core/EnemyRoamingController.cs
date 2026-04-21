using UnityEngine;

[RequireComponent(typeof(EnemyAIController))]
public class EnemyRoamingController : MonoBehaviour
{
    [Header("Roaming")]
    [SerializeField] private float roamRadius = 3f;
    [SerializeField] private float roamMoveSpeed = 1.5f;
    [SerializeField] private float minPauseTime = 1.5f;
    [SerializeField] private float maxPauseTime = 3.5f;
    [SerializeField] private float waypointTolerance = 0.15f;

    private EnemyAIController enemyAI;

    private Vector3 currentRoamTarget;
    private bool hasRoamTarget = false;
    private float pauseTimer = 0f;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAIController>();
        ResetPauseTimer();
    }

    private void Update()
    {
        if (enemyAI == null || enemyAI.IsBusy())
        {
            hasRoamTarget = false;
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        if (!hasRoamTarget)
        {
            PickNewRoamTarget();
        }

        MoveTowardRoamTarget();
    }

    private void PickNewRoamTarget()
    {
        Vector3 home = enemyAI.GetHomePosition();
        Vector2 randomCircle = Random.insideUnitCircle * roamRadius;

        currentRoamTarget = new Vector3(
            home.x + randomCircle.x,
            transform.position.y,
            home.z + randomCircle.y
        );

        hasRoamTarget = true;
    }

    private void MoveTowardRoamTarget()
    {
        Vector3 dir = currentRoamTarget - transform.position;
        dir.y = 0f;

        if (dir.magnitude <= waypointTolerance)
        {
            hasRoamTarget = false;
            ResetPauseTimer();
            return;
        }

        dir.Normalize();
        transform.position += dir * roamMoveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 6f * Time.deltaTime);
    }

    private void ResetPauseTimer()
    {
        pauseTimer = Random.Range(minPauseTime, maxPauseTime);
    }
}