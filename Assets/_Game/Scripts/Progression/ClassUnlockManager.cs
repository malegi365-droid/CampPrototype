using UnityEngine;

public enum PlayerClassType
{
    DPS,
    Tank,
    Healer
}

public class ClassUnlockManager : MonoBehaviour
{
    public static ClassUnlockManager Instance { get; private set; }

    [Header("Starting Unlocks")]
    [SerializeField] private bool dpsUnlocked = true;
    [SerializeField] private bool tankUnlocked = false;
    [SerializeField] private bool healerUnlocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // Debug testing only
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UnlockClass(PlayerClassType.Tank);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            UnlockClass(PlayerClassType.Healer);
        }
    }

    public bool IsClassUnlocked(PlayerClassType classType)
    {
        switch (classType)
        {
            case PlayerClassType.DPS:
                return dpsUnlocked;

            case PlayerClassType.Tank:
                return tankUnlocked;

            case PlayerClassType.Healer:
                return healerUnlocked;

            default:
                return false;
        }
    }

    public void UnlockClass(PlayerClassType classType)
    {
        switch (classType)
        {
            case PlayerClassType.DPS:
                dpsUnlocked = true;
                break;

            case PlayerClassType.Tank:
                tankUnlocked = true;
                break;

            case PlayerClassType.Healer:
                healerUnlocked = true;
                break;
        }

        Debug.Log($"Unlocked class: {classType}");
    }
}