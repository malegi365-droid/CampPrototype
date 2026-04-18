using UnityEngine;

public class TempTargetSetter : MonoBehaviour
{
    public Transform target;

    private void Start()
    {
        if (target == null) return;

        AutoAttackController attackController = GetComponent<AutoAttackController>();
        if (attackController != null)
        {
            attackController.SetTarget(target);
        }

        TargetingController targetingController = GetComponent<TargetingController>();
        if (targetingController != null)
        {
            targetingController.SetTarget(target);
        }
    }
}