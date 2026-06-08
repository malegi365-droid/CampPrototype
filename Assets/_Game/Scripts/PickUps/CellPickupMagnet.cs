using UnityEngine;

public class CellPickupMagnet : MonoBehaviour
{
    [Header("Magnet")]
    [SerializeField] private float magnetRange = 4f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 18f;

    private Transform player;
    private float currentSpeed;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(transform.position, player.position);

        if (distance > magnetRange)
            return;

        currentSpeed += acceleration * Time.deltaTime;

        Vector3 direction =
            (player.position - transform.position).normalized;

        transform.position +=
            direction * currentSpeed * Time.deltaTime;
    }
}