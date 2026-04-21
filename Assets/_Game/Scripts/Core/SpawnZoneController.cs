using System.Collections.Generic;
using UnityEngine;

public class SpawnZoneController : MonoBehaviour
{
    [Header("Zone Setup")]
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
    [SerializeField] private List<EnemyGroupTemplate> possibleGroups = new List<EnemyGroupTemplate>();

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool randomizeGroups = true;

    private readonly List<GameObject> spawnedRootGroups = new List<GameObject>();

    private void Start()
    {
        Debug.Log($"{gameObject.name}: Start called. SpawnPoints={spawnPoints.Count}, PossibleGroups={possibleGroups.Count}");

        if (spawnOnStart)
        {
            SpawnAllPoints();
        }
    }

    public void SpawnAllPoints()
    {
        ClearSpawnedGroups();

        Debug.Log($"{gameObject.name}: SpawnAllPoints called.");

        if (spawnPoints.Count == 0 || possibleGroups.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: Missing spawn points or group templates.");
            return;
        }

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Debug.Log($"{gameObject.name}: SpawnPoint[{i}] = {(spawnPoints[i] == null ? "NULL" : spawnPoints[i].name)}");
        }

        for (int i = 0; i < possibleGroups.Count; i++)
        {
            Debug.Log($"{gameObject.name}: PossibleGroup[{i}] = {(possibleGroups[i] == null ? "NULL" : possibleGroups[i].groupName)}");
        }

        foreach (SpawnPoint point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogWarning($"{gameObject.name}: Encountered NULL spawn point.");
                continue;
            }

            EnemyGroupTemplate chosenGroup = ChooseGroupTemplate();
            if (chosenGroup == null)
            {
                Debug.LogWarning($"{gameObject.name}: Chosen group was NULL.");
                continue;
            }

            SpawnGroupAtPoint(chosenGroup, point);
        }
    }

    private EnemyGroupTemplate ChooseGroupTemplate()
    {
        if (possibleGroups.Count == 0)
            return null;

        if (!randomizeGroups)
            return possibleGroups[0];

        int index = Random.Range(0, possibleGroups.Count);
        return possibleGroups[index];
    }

    private void SpawnGroupAtPoint(EnemyGroupTemplate groupTemplate, SpawnPoint point)
    {
        Debug.Log($"{gameObject.name}: Spawning group '{groupTemplate.groupName}' at point '{point.name}'");

        GameObject groupRoot = new GameObject($"{groupTemplate.groupName}_Spawned");
        groupRoot.transform.SetParent(transform);
        groupRoot.transform.position = point.GetSpawnPosition();

        spawnedRootGroups.Add(groupRoot);

        Vector3 anchor = point.GetSpawnPosition();
        int runningIndex = 0;

        foreach (EnemySpawnEntry entry in groupTemplate.entries)
        {
            if (entry == null)
            {
                Debug.LogWarning($"{gameObject.name}: Encountered NULL EnemySpawnEntry in group '{groupTemplate.groupName}'");
                continue;
            }

            if (entry.enemyPrefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: Entry in group '{groupTemplate.groupName}' has NULL enemyPrefab");
                continue;
            }

            if (entry.count <= 0)
            {
                Debug.LogWarning($"{gameObject.name}: Entry in group '{groupTemplate.groupName}' has count <= 0");
                continue;
            }

            Debug.Log($"{gameObject.name}: Entry prefab={entry.enemyPrefab.name}, count={entry.count}, spacing={entry.spacing}");

            for (int i = 0; i < entry.count; i++)
            {
                Vector3 offset = GetOffset(runningIndex, entry.spacing);
                Vector3 spawnPos = anchor + offset;

                GameObject spawned = Instantiate(entry.enemyPrefab, spawnPos, Quaternion.identity, groupRoot.transform);
                spawned.name = entry.enemyPrefab.name;

                Debug.Log($"{gameObject.name}: Spawned {spawned.name} at {spawnPos}");

                runningIndex++;
            }
        }
    }

    private Vector3 GetOffset(int index, float spacing)
    {
        if (index == 0) return Vector3.zero;

        int ring = (index / 6) + 1;
        int slot = index % 6;
        float angle = slot * 60f * Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spacing * ring;
    }

    public void ClearSpawnedGroups()
    {
        for (int i = spawnedRootGroups.Count - 1; i >= 0; i--)
        {
            if (spawnedRootGroups[i] != null)
            {
                Destroy(spawnedRootGroups[i]);
            }
        }

        spawnedRootGroups.Clear();
    }
}