using UnityEngine;

public class DPSInjectorProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 travelDirection;

    public void Initialize(Vector3 direction)
    {
        travelDirection = direction.normalized;

        if (travelDirection.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);
    }

    private void Start()
    {
        if (travelDirection.sqrMagnitude <= 0.001f)
            travelDirection = transform.forward;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += travelDirection * speed * Time.deltaTime;
    }
}