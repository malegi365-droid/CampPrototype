using UnityEngine;

public class CampRecoveryZone : MonoBehaviour
{
    [Header("Party References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject healer;

    [Header("Behavior")]
    [SerializeField] private bool clearTargetsOnRecover = true;
    [SerializeField] private bool logRecovery = true;

    private void OnTriggerEnter(Collider other)
    {
        if (player == null) return;

        if (other.gameObject != player)
            return;

        RecoverParty();
    }

    private void RecoverParty()
    {
        RestoreUnit(player);
        RestoreUnit(tank);
        RestoreUnit(healer);

        if (clearTargetsOnRecover)
        {
            ClearTarget(player);
            ClearTarget(tank);
            ClearTarget(healer);
        }

        if (logRecovery)
        {
            Debug.Log("Party recovered at camp.");
        }
    }

    private void RestoreUnit(GameObject unit)
    {
        if (unit == null) return;

        HealthController health = unit.GetComponent<HealthController>();
        if (health != null)
        {
            health.ResetHealth();
        }
    }

    private void ClearTarget(GameObject unit)
    {
        if (unit == null) return;

        TargetingController targeting = unit.GetComponent<TargetingController>();
        if (targeting != null)
        {
            targeting.ClearTarget();
        }

        AutoAttackController autoAttack = unit.GetComponent<AutoAttackController>();
        if (autoAttack != null)
        {
            autoAttack.SetTarget(null);
        }
    }
}