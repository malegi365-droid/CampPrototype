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

    private const string DPS_KEY = "CLASS_UNLOCK_DPS";
    private const string TANK_KEY = "CLASS_UNLOCK_TANK";
    private const string HEALER_KEY = "CLASS_UNLOCK_HEALER";

    private bool dpsUnlocked;
    private bool tankUnlocked;
    private bool healerUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadUnlocks();
    }

    private void Update()
    {
        // TEMP DEV RESET
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetUnlocks();
        }
    }

    private void LoadUnlocks()
    {
        dpsUnlocked = PlayerPrefs.GetInt(DPS_KEY, 1) == 1;
        tankUnlocked = PlayerPrefs.GetInt(TANK_KEY, 0) == 1;
        healerUnlocked = PlayerPrefs.GetInt(HEALER_KEY, 0) == 1;
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
                PlayerPrefs.SetInt(DPS_KEY, 1);
                break;

            case PlayerClassType.Tank:
                tankUnlocked = true;
                PlayerPrefs.SetInt(TANK_KEY, 1);
                break;

            case PlayerClassType.Healer:
                healerUnlocked = true;
                PlayerPrefs.SetInt(HEALER_KEY, 1);
                break;
        }

        PlayerPrefs.Save();
        Debug.Log($"Unlocked class: {classType}");
    }

    public void ResetUnlocks()
    {
        PlayerPrefs.DeleteKey(TANK_KEY);
        PlayerPrefs.DeleteKey(HEALER_KEY);

        LoadUnlocks();

        Debug.Log("[ClassUnlockManager] Class unlocks reset. DPS remains unlocked.");
    }
}