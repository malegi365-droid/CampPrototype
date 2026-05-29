using UnityEngine;

public class PickupHover : MonoBehaviour
{
    [Header("Hover")]
    [SerializeField] private float hoverHeight = 0.18f;
    [SerializeField] private float hoverSpeed = 2f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 90f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        Hover();
        Rotate();
    }

    private void Hover()
    {
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;

        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + yOffset,
            startPosition.z
        );
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}