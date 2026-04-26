using UnityEngine;

public class PartyTargetCoordinator : MonoBehaviour
{
    [Header("Party Control")]
    [SerializeField] private PartyControlManager partyControlManager;

    [Header("Party Members")]
    [SerializeField] private PartyMemberControlBridge tank;
    [SerializeField] private PartyMemberControlBridge dps;
    [SerializeField] private PartyMemberControlBridge healer;

    [Header("Targeting Controllers")]
    [SerializeField] private TargetingController tankTargeting;
    [SerializeField] private TargetingController dpsTargeting;
    [SerializeField] private TargetingController healerTargeting;

    [Header("Settings")]
    [SerializeField] private bool syncTankAndDpsTargets = true;
    [SerializeField] private bool clearInactiveHealerTarget = true;

    private Transform lastSyncedTarget;

    private void Awake()
    {
        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();

        AutoFillReferences();
    }

    private void Update()
    {
        if (partyControlManager == null || partyControlManager.CurrentMember == null)
            return;

        if (!syncTankAndDpsTargets)
            return;

        TargetingController leaderTargeting = partyControlManager.CurrentMember.GetComponent<TargetingController>();

        if (leaderTargeting == null)
            return;

        Transform leaderTarget = leaderTargeting.GetCurrentTarget();

        if (!IsValidTarget(leaderTarget))
            leaderTarget = null;

        SyncTankAndDpsToTarget(leaderTarget);

        if (clearInactiveHealerTarget && healer != null && !healer.IsPlayerControlled && healerTargeting != null)
        {
            healerTargeting.ClearTarget();
        }

        lastSyncedTarget = leaderTarget;
    }

    private void SyncTankAndDpsToTarget(Transform target)
    {
        PartyMemberControlBridge current = partyControlManager.CurrentMember;

        if (tank != null && tankTargeting != null && tank != current)
        {
            SetOrClearTarget(tankTargeting, target);
        }

        if (dps != null && dpsTargeting != null && dps != current)
        {
            SetOrClearTarget(dpsTargeting, target);
        }
    }

    private void SetOrClearTarget(TargetingController controller, Transform target)
    {
        if (controller == null)
            return;

        Transform currentTarget = controller.GetCurrentTarget();

        if (target == null)
        {
            if (currentTarget != null)
                controller.ClearTarget();

            return;
        }

        if (currentTarget != target)
            controller.SetTarget(target);
    }

    private bool IsValidTarget(Transform target)
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        HealthController health = target.GetComponent<HealthController>();

        if (health != null && health.IsDead())
            return false;

        UnitStats stats = target.GetComponent<UnitStats>();

        if (stats == null)
            return false;

        return stats.role == UnitRole.Enemy;
    }

    private void AutoFillReferences()
    {
        if (tank != null && tankTargeting == null)
            tankTargeting = tank.GetComponent<TargetingController>();

        if (dps != null && dpsTargeting == null)
            dpsTargeting = dps.GetComponent<TargetingController>();

        if (healer != null && healerTargeting == null)
            healerTargeting = healer.GetComponent<TargetingController>();
    }
}