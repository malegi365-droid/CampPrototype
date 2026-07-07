using System.Collections;
using UnityEngine;

public class AbilityWeaveShowcaseDirector : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode startKey = KeyCode.P;

    [Header("Player Objects")]
    [SerializeField] private GameObject rangerObject;
    [SerializeField] private GameObject guardianObject;

    [Header("Showcase API")]
    [SerializeField] private RangerProjectileFireController rangerFire;
    [SerializeField] private PlayerMovementController rangerMovement;
    [SerializeField] private GuardianMeteorDiveAbility meteorDiveAbility;

    [Header("Enemy Setup")]
    [SerializeField] private GameObject easyEnemyPrefab;
    [SerializeField] private int enemyCount = 6;
    [SerializeField] private float enemyDistanceInFront = 5f;
    [SerializeField] private float enemySpacing = 1.1f;

    [Header("Player Setup")]
    [SerializeField] private float rangerDistanceBehindGuardian = 4f;

    [Header("Timing")]
    [SerializeField] private float openingPause = 0.5f;
    [SerializeField] private float timeBetweenShots = 0.35f;
    [SerializeField] private float afterSecondShotPause = 0.2f;
    [SerializeField] private float afterRollPause = 0.45f;
    [SerializeField] private float afterSwitchPause = 0.2f;
    [SerializeField] private float afterMeteorDivePause = 3f;

    [Header("Debug")]
    [SerializeField] private bool logSteps = true;

    private bool isRunning;
    private GameObject[] spawnedEnemies;

    private void Update()
    {
        if (Input.GetKeyDown(startKey) && !isRunning)
            StartCoroutine(RunMeteorDiveShowcase());
    }

    private IEnumerator RunMeteorDiveShowcase()
    {
        isRunning = true;

        Log("Showcase started.");

        Vector3 forward = Vector3.right;

        Vector3 guardianPosition = guardianObject != null ? guardianObject.transform.position : transform.position;
        Quaternion forwardRotation = Quaternion.LookRotation(forward, Vector3.up);

        Vector3 rangerPosition = guardianPosition - forward * rangerDistanceBehindGuardian;
        Vector3 enemyCenter = guardianPosition + forward * enemyDistanceInFront;

        SpawnEnemyCluster(enemyCenter, Quaternion.LookRotation(-forward, Vector3.up));

        ActivateRanger(rangerPosition, forwardRotation);

        yield return new WaitForSeconds(openingPause);

        Log("Ranger shot 1.");
        rangerFire?.ForceBasicFireForShowcase(enemyCenter);

        yield return new WaitForSeconds(timeBetweenShots);

        Log("Ranger shot 2.");
        rangerFire?.ForceBasicFireForShowcase(enemyCenter);

        yield return new WaitForSeconds(afterSecondShotPause);

        Log("Ranger roll.");
        rangerMovement?.ForceDashForShowcase(forward);

        yield return new WaitForSeconds(afterRollPause);

        Log("Switch to Guardian.");

        Vector3 rangerFinalPosition = rangerObject != null
            ? rangerObject.transform.position
            : guardianPosition;

        ActivateGuardian(rangerFinalPosition, forwardRotation);

        AbilityWeaveManager.Instance?.ForceMeteorDiveReadyForShowcase();

        yield return new WaitForSeconds(afterSwitchPause);

        Log("Meteor Dive.");
        meteorDiveAbility?.ForceMeteorDiveForShowcase();

        yield return new WaitForSeconds(afterMeteorDivePause);

        Log("Showcase complete.");
        isRunning = false;
    }

    private void ActivateRanger(Vector3 position, Quaternion rotation)
    {
        if (guardianObject != null)
            guardianObject.SetActive(false);

        if (rangerObject != null)
        {
            rangerObject.SetActive(true);
            rangerObject.transform.position = position;
            rangerObject.transform.rotation = rotation;
        }
    }

    private void ActivateGuardian(Vector3 position, Quaternion rotation)
    {
        if (rangerObject != null)
            rangerObject.SetActive(false);

        if (guardianObject != null)
        {
            guardianObject.SetActive(true);
            guardianObject.transform.position = position;
            guardianObject.transform.rotation = rotation;
        }
    }

    private void SpawnEnemyCluster(Vector3 center, Quaternion rotation)
    {
        ClearSpawnedEnemies();

        if (easyEnemyPrefab == null)
        {
            Debug.LogWarning("[AbilityWeaveShowcaseDirector] Missing Easy Enemy Prefab.");
            return;
        }

        spawnedEnemies = new GameObject[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            int row = i / 3;
            int column = i % 3;

            Vector3 offset = new Vector3(
                (column - 1) * enemySpacing,
                0f,
                row * enemySpacing
            );

            GameObject enemy = Instantiate(
                easyEnemyPrefab,
                center + offset,
                rotation
            );

            spawnedEnemies[i] = enemy;
            DisableEnemyMovement(enemy);
        }

        Log($"Spawned {enemyCount} enemies.");
    }

    private void ClearSpawnedEnemies()
    {
        if (spawnedEnemies == null)
            return;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        spawnedEnemies = null;
    }

    private void DisableEnemyMovement(GameObject enemy)
    {
        MonoBehaviour[] behaviours = enemy.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;

            if (typeName.Contains("AI") ||
                typeName.Contains("Roaming") ||
                typeName.Contains("AutoAttack") ||
                typeName.Contains("Movement"))
            {
                behaviour.enabled = false;
            }
        }
    }

    private void Log(string message)
    {
        if (logSteps)
            Debug.Log("[AbilityWeaveShowcaseDirector] " + message);
    }
}