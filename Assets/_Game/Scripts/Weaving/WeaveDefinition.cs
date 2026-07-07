using System;
using UnityEngine;

[Serializable]
public class WeaveDefinition
{
    public string weaveName;

    public CombatClassType firstClass;
    public AbilitySlotType firstAbility;

    public CombatClassType secondClass;
    public AbilitySlotType secondAbility;

    public float maxTimeBetweenAbilities = 3f;
}