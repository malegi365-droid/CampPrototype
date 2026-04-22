using System.Collections.Generic;
using UnityEngine;

public class SpawnedEnemyGroup : MonoBehaviour
{
    private float respawnDelay = 12f;

    private SpawnZoneController ownerZone;
    private EnemyGroupTemplate sourceTemplate;
    private SpawnPoint sourceSpawnPoint;

    private readonly List<HealthController> members = new List<HealthController>();
    private bool respawnScheduled = false;

    public void Initialize(
        SpawnZoneController zone,
        EnemyGroupTemplate template,
        SpawnPoint spawnPoint,
        List<GameObject> spawnedMembers,
        float delay)
    {
        ownerZone = zone;
        sourceTemplate = template;
        sourceSpawnPoint = spawnPoint;
        respawnDelay = delay;

        members.Clear();

        foreach (GameObject memberObj in spawnedMembers)
        {
            if (memberObj == null) continue;

            HealthController hc = memberObj.GetComponent<HealthController>();
            if (hc == null) continue;

            members.Add(hc);
            hc.OnDied += HandleMemberDied;
        }
    }

    private void HandleMemberDied(HealthController deadMember)
    {
        if (respawnScheduled) return;

        if (AllMembersDead())
        {
            respawnScheduled = true;
            Debug.Log($"{gameObject.name}: all members dead, respawning in {respawnDelay} seconds.");
            Invoke(nameof(RequestRespawn), respawnDelay);
        }
    }

    private bool AllMembersDead()
    {
        foreach (HealthController member in members)
        {
            if (member != null && !member.IsDead())
                return false;
        }

        return true;
    }

    private void RequestRespawn()
    {
        CleanupSubscriptions();

        if (ownerZone != null && sourceTemplate != null && sourceSpawnPoint != null)
        {
            ownerZone.RespawnGroup(sourceTemplate, sourceSpawnPoint);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CleanupSubscriptions();
    }

    private void CleanupSubscriptions()
    {
        foreach (HealthController member in members)
        {
            if (member != null)
            {
                member.OnDied -= HandleMemberDied;
            }
        }
    }
}