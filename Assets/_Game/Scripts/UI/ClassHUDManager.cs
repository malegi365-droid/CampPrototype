using UnityEngine;

public class ClassHUDManager : MonoBehaviour
{
    [Header("Class HUD Roots")]
    [SerializeField] private GameObject guardianHUD;
    [SerializeField] private GameObject rangerHUD;
    [SerializeField] private GameObject toxicologistHUD;

    public void ShowHUD(PlayerClassType classType)
    {
        if (guardianHUD != null)
            guardianHUD.SetActive(classType == PlayerClassType.Tank);

        if (rangerHUD != null)
            rangerHUD.SetActive(classType == PlayerClassType.DPS);

        if (toxicologistHUD != null)
            toxicologistHUD.SetActive(classType == PlayerClassType.Healer);
    }
}