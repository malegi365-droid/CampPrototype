using UnityEngine;

public class FloatingFragmentMotion : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private float bobSpeed = 0.8f;

    [SerializeField]
    private Vector3 rotationSpeed =
        new Vector3(0f, 12f, 4f);

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float bob =
            Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.position =
            startPosition + Vector3.up * bob;

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}