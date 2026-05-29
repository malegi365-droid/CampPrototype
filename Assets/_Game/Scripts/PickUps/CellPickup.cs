using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CellPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int cellValue = 1;

    [Header("Motion")]
    [SerializeField] private float rotateSpeed = 90f;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CellResourceManager.Instance != null)
        {
            CellResourceManager.Instance.AddCells(cellValue);
        }

        Destroy(gameObject);
    }
}