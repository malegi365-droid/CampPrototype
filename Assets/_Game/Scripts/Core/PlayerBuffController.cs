using UnityEngine;

public class PlayerBuffController : MonoBehaviour
{
    public static PlayerBuffController Instance { get; private set; }

    [Header("Guardian Shield")]
    [SerializeField] private bool guardianShieldActive = false;
    [SerializeField] private float guardianShieldDamageReduction = 0.5f;

    [Header("Guardian Last Stand")]
    [SerializeField] private bool guardianLastStandActive = false;
    [SerializeField] private float guardianLastStandDamageReduction = 0.75f;

    [Header("Guardian Shield VFX")]
    [SerializeField] private GameObject guardianShieldVFXPrefab;
    [SerializeField] private Vector3 shieldVFXOffset = new Vector3(0f, 1f, 0f);

    [Header("Rapid Regen")]
    [SerializeField] private bool rapidRegenActive = false;
    [SerializeField] private float rapidRegenHealPerTick = 5f;
    [SerializeField] private float rapidRegenTickInterval = 1f;

    [Header("Rapid Regen VFX")]
    [SerializeField] private GameObject rapidRegenVFXPrefab;
    [SerializeField] private Vector3 rapidRegenVFXOffset = new Vector3(0f, 1f, 0f);

    private float shieldEndTime;
    private GameObject activeShieldVFX;

    private float rapidRegenEndTime;
    private float nextRapidRegenTickTime;
    private GameObject activeRapidRegenVFX;

    private PartyControlManager partyControlManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        partyControlManager = FindAnyObjectByType<PartyControlManager>();
    }

    private void Update()
    {
        UpdateGuardianShield();
        UpdateRapidRegen();
    }

    public void ActivateGuardianShield(float duration, float damageReduction)
    {
        guardianShieldActive = true;
        guardianShieldDamageReduction = Mathf.Clamp01(damageReduction);
        shieldEndTime = Time.time + duration;

        SpawnShieldVFX();

        Debug.Log("[PlayerBuffController] Guardian Shield active.");
    }

    public void SetGuardianLastStandActive(bool active, float damageReduction = 0.75f)
    {
        guardianLastStandActive = active;
        guardianLastStandDamageReduction = Mathf.Clamp01(damageReduction);

        Debug.Log($"[PlayerBuffController] Guardian Last Stand active: {guardianLastStandActive}. DamageReduction={guardianLastStandDamageReduction}");
    }

    public void ActivateRapidRegen(float duration, float healPerTick, float tickInterval)
    {
        rapidRegenActive = true;
        rapidRegenHealPerTick = healPerTick;
        rapidRegenTickInterval = Mathf.Max(0.1f, tickInterval);
        rapidRegenEndTime = Time.time + duration;
        nextRapidRegenTickTime = Time.time;

        SpawnRapidRegenVFX();

        Debug.Log("[PlayerBuffController] Rapid Regen active.");
    }

    public float ModifyIncomingPlayerDamage(float damage)
    {
        float modifiedDamage = damage;

        if (guardianShieldActive)
            modifiedDamage *= (1f - guardianShieldDamageReduction);

        if (guardianLastStandActive)
            modifiedDamage *= (1f - guardianLastStandDamageReduction);

        return modifiedDamage;
    }

    public bool IsGuardianShieldActive()
    {
        return guardianShieldActive;
    }

    public bool IsGuardianLastStandActive()
    {
        return guardianLastStandActive;
    }

    public bool IsRapidRegenActive()
    {
        return rapidRegenActive;
    }

    private void UpdateGuardianShield()
    {
        if (!guardianShieldActive)
            return;

        FollowShieldVFX();

        if (Time.time >= shieldEndTime)
            EndGuardianShield();
    }

    private void UpdateRapidRegen()
    {
        if (!rapidRegenActive)
            return;

        FollowRapidRegenVFX();

        if (Time.time >= rapidRegenEndTime)
        {
            EndRapidRegen();
            return;
        }

        if (Time.time >= nextRapidRegenTickTime)
        {
            HealActivePlayer();
            nextRapidRegenTickTime = Time.time + rapidRegenTickInterval;
        }
    }

    private void HealActivePlayer()
    {
        PartyMemberControlBridge activeMember = GetActivePlayerMember();

        if (activeMember == null)
            return;

        HealthController health = activeMember.GetComponent<HealthController>();

        if (health == null)
            health = activeMember.GetComponentInChildren<HealthController>();

        if (health == null)
            return;

        health.ReceiveHealing(rapidRegenHealPerTick);

        DamageNumberSpawner.ShowHealing(
            activeMember.transform.position,
            rapidRegenHealPerTick
        );

        Debug.Log($"[PlayerBuffController] Rapid Regen healed {activeMember.RoleName} for {rapidRegenHealPerTick}.");
    }

    private void SpawnShieldVFX()
    {
        if (guardianShieldVFXPrefab == null)
            return;

        if (activeShieldVFX != null)
            Destroy(activeShieldVFX);

        Transform activeTarget = GetActivePlayerTransform();

        if (activeTarget == null)
            return;

        activeShieldVFX = Instantiate(
            guardianShieldVFXPrefab,
            activeTarget.position + shieldVFXOffset,
            Quaternion.identity
        );
    }

    private void SpawnRapidRegenVFX()
    {
        if (rapidRegenVFXPrefab == null)
            return;

        if (activeRapidRegenVFX != null)
            Destroy(activeRapidRegenVFX);

        Transform activeTarget = GetActivePlayerTransform();

        if (activeTarget == null)
            return;

        activeRapidRegenVFX = Instantiate(
            rapidRegenVFXPrefab,
            activeTarget.position + rapidRegenVFXOffset,
            Quaternion.identity
        );
    }

    private void FollowShieldVFX()
    {
        if (activeShieldVFX == null)
            return;

        Transform activeTarget = GetActivePlayerTransform();

        if (activeTarget == null)
            return;

        activeShieldVFX.transform.position =
            activeTarget.position + shieldVFXOffset;
    }

    private void FollowRapidRegenVFX()
    {
        if (activeRapidRegenVFX == null)
            return;

        Transform activeTarget = GetActivePlayerTransform();

        if (activeTarget == null)
            return;

        activeRapidRegenVFX.transform.position =
            activeTarget.position + rapidRegenVFXOffset;
    }

    private PartyMemberControlBridge GetActivePlayerMember()
    {
        if (partyControlManager == null)
            partyControlManager = FindAnyObjectByType<PartyControlManager>();

        if (partyControlManager == null)
            return null;

        return partyControlManager.CurrentMember;
    }

    private Transform GetActivePlayerTransform()
    {
        PartyMemberControlBridge activeMember = GetActivePlayerMember();

        if (activeMember == null)
            return null;

        return activeMember.transform;
    }

    private void EndGuardianShield()
    {
        guardianShieldActive = false;

        if (activeShieldVFX != null)
            Destroy(activeShieldVFX);

        Debug.Log("[PlayerBuffController] Guardian Shield ended.");
    }

    private void EndRapidRegen()
    {
        rapidRegenActive = false;

        if (activeRapidRegenVFX != null)
            Destroy(activeRapidRegenVFX);

        Debug.Log("[PlayerBuffController] Rapid Regen ended.");
    }
}