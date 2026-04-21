using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float gizmoRadius = 0.4f;

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}