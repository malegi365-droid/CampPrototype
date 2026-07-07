using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityWeaveManager : MonoBehaviour
{
    public static AbilityWeaveManager Instance { get; private set; }

    [Header("Weave Definitions")]
    [SerializeField] private List<WeaveDefinition> weaveDefinitions = new();

    [Header("Technique Ready Windows")]
    [SerializeField] private float meteorDiveReadyWindow = 3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private CombatClassType lastClass;
    private AbilitySlotType lastAbility;
    private float lastAbilityTime;
    private bool hasPreviousAbility;

    private bool meteorDiveReady;
    private float meteorDiveExpireTime;

    public bool IsMeteorDiveReady =>
        meteorDiveReady && Time.time <= meteorDiveExpireTime;

    public event Action<string> OnWeaveTriggered;

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
        if (meteorDiveReady && Time.time > meteorDiveExpireTime)
        {
            meteorDiveReady = false;

            if (debugLogs)
                Debug.Log("[AbilityWeaveManager] Meteor Dive ready expired.");
        }
    }

    public void RecordAbilityUsed(CombatClassType usedClass, AbilitySlotType usedAbility)
    {
        float currentTime = Time.time;

        if (debugLogs)
            Debug.Log($"Ability Used: {usedClass} - {usedAbility}");

        if (usedClass == CombatClassType.Ranger &&
            usedAbility == AbilitySlotType.Movement)
        {
            ArmMeteorDive();
        }

        if (hasPreviousAbility)
            CheckForWeave(usedClass, usedAbility, currentTime);

        lastClass = usedClass;
        lastAbility = usedAbility;
        lastAbilityTime = currentTime;
        hasPreviousAbility = true;
    }

    public bool ConsumeMeteorDiveReady()
    {
        if (!IsMeteorDiveReady)
            return false;

        meteorDiveReady = false;

        if (debugLogs)
            Debug.Log("[AbilityWeaveManager] Meteor Dive consumed.");

        return true;
    }

    public void ForceMeteorDiveReadyForShowcase()
    {
        meteorDiveReady = true;
        meteorDiveExpireTime = Time.time + meteorDiveReadyWindow;

        if (debugLogs)
            Debug.Log("[AbilityWeaveManager] Meteor Dive forced ready for showcase.");
    }

    private void ArmMeteorDive()
    {
        meteorDiveReady = true;
        meteorDiveExpireTime = Time.time + meteorDiveReadyWindow;

        if (debugLogs)
            Debug.Log("[AbilityWeaveManager] Meteor Dive ready.");
    }

    private void CheckForWeave(
        CombatClassType currentClass,
        AbilitySlotType currentAbility,
        float currentTime)
    {
        foreach (WeaveDefinition weave in weaveDefinitions)
        {
            bool firstMatches =
                weave.firstClass == lastClass &&
                weave.firstAbility == lastAbility;

            bool secondMatches =
                weave.secondClass == currentClass &&
                weave.secondAbility == currentAbility;

            bool withinTime =
                currentTime - lastAbilityTime <= weave.maxTimeBetweenAbilities;

            bool changedClass =
                lastClass != currentClass;

            if (firstMatches && secondMatches && withinTime && changedClass)
            {
                TriggerWeave(weave.weaveName);
                return;
            }
        }
    }

    private void TriggerWeave(string weaveName)
    {
        if (debugLogs)
            Debug.Log($"WEAVE TRIGGERED: {weaveName}");

        OnWeaveTriggered?.Invoke(weaveName);
    }
}