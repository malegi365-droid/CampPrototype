using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CellPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int cellValue = 1;

    [Header("Motion")]
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Pickup Feedback")]
    [SerializeField] private GameObject pickupBurstPrefab;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float pickupSoundVolume = 0.8f;

    private bool collected = false;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        collected = true;

        if (CellResourceManager.Instance != null)
        {
            CellResourceManager.Instance.AddCells(cellValue);
        }

        if (pickupBurstPrefab != null)
        {
            Instantiate(
                pickupBurstPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position,
                pickupSoundVolume
            );
        }

        Destroy(gameObject);
    }
}