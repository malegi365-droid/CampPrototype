using UnityEngine;
using System.Collections;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController Instance { get; private set; }

    [SerializeField] private float defaultDuration = 0.08f;
    [SerializeField] private float defaultMagnitude = 0.08f;

    private Vector3 originalLocalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        Instance = this;
        originalLocalPosition = transform.localPosition;
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 random = Random.insideUnitCircle * magnitude;
            transform.localPosition = originalLocalPosition + new Vector3(random.x, random.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        shakeRoutine = null;
    }
}