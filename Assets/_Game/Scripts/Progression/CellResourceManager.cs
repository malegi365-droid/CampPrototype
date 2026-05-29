using TMPro;
using UnityEngine;

public class CellResourceManager : MonoBehaviour
{
    public static CellResourceManager Instance;

    [Header("Resources")]
    [SerializeField] private int currentCells = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text cellText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateUI();
    }

    public void AddCells(int amount)
    {
        currentCells += amount;

        Debug.Log($"Cells gained: {amount} | Total: {currentCells}");

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cellText != null)
        {
            cellText.text = $"CELLS: {currentCells:00}";
        }
    }
}