using UnityEngine;

public class PickupHoverMotion : MonoBehaviour
{
    [SerializeField] private float hoverHeight = 0.2f;
    [SerializeField] private float hoverSpeed = 2f;

    private Vector3 startPosition;
    private float randomOffset;

    private void Awake()
    {
        startPosition = transform.position;

        // Slight random offset so pickups don't all hover identically.
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float hover =
            Mathf.Sin((Time.time + randomOffset) * hoverSpeed)
            * hoverHeight;

        transform.position =
            startPosition + Vector3.up * hover;
    }
}