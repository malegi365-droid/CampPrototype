using UnityEngine;

public class CampRecoveryZone : MonoBehaviour
{
    [Header("Class References")]
    [SerializeField] private GameObject dps;
    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject healer;

    [Header("Behavior")]
    [SerializeField] private bool clearTargetsOnRecover = true;
    [SerializeField] private bool logRecovery = true;

    private void OnTriggerEnter(Collider other)
    {
        TryRecoverFromCollider(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryRecoverFromCollider(other);
    }

    private void TryRecoverFromCollider(Collider other)
    {
        PartyMemberControlBridge bridge = other.GetComponent<PartyMemberControlBridge>();

        if (bridge == null)
            bridge = other.GetComponentInParent<PartyMemberControlBridge>();

        if (bridge == null || !bridge.IsPlayerControlled)
            return;

        RecoverParty();
    }

    private void RecoverParty()
    {
        RestoreUnit(dps);
        RestoreUnit(tank);
        RestoreUnit(healer);

        if (clearTargetsOnRecover)
        {
            ClearTarget(dps);
            ClearTarget(tank);
            ClearTarget(healer);
        }

        if (logRecovery)
            Debug.Log("Party recovered at camp.");
    }

    private void RestoreUnit(GameObject unit)
    {
        if (unit == null) return;

        HealthController health = unit.GetComponent<HealthController>();
        if (health != null)
            health.ResetHealth();
    }

    private void ClearTarget(GameObject unit)
    {
        if (unit == null) return;

        TargetingController targeting = unit.GetComponent<TargetingController>();
        if (targeting != null)
            targeting.ClearTarget();

        AutoAttackController autoAttack = unit.GetComponent<AutoAttackController>();
        if (autoAttack != null)
            autoAttack.SetTarget(null);
    }
}