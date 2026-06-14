using UnityEngine;

public class ProjectileWobble : MonoBehaviour
{
    [SerializeField] private float wobbleAmount = 8f;
    [SerializeField] private float wobbleSpeed = 8f;

    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float x = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        float y = Mathf.Cos(Time.time * wobbleSpeed * 0.8f) * wobbleAmount;

        transform.localRotation =
            startRotation * Quaternion.Euler(x, y, 0f);
    }
}