using UnityEngine;
using System.Collections;

public class CameraShakeController : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.08f;
    [SerializeField] private float defaultStrength = 0.08f;

    private Coroutine shakeRoutine;

    public void Shake()
    {
        Shake(defaultDuration, defaultStrength);
    }

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength, transform.localPosition));
    }

    private IEnumerator ShakeRoutine(float duration, float strength, Vector3 baseLocalPosition)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 randomCircle = Random.insideUnitCircle * strength;

            Vector3 offset = new Vector3(
                randomCircle.x,
                randomCircle.y,
                0f
            );

            transform.localPosition = baseLocalPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = baseLocalPosition;
        shakeRoutine = null;
    }
}