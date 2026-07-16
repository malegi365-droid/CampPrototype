using System.Collections;
using UnityEngine;

public class VolatileReactionShowcaseDirector : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode startKey = KeyCode.P;

    [Header("Player Objects")]
    [SerializeField] private GameObject toxicologistObject;
    [SerializeField] private GameObject rangerObject;
    [SerializeField] private GameObject guardianObject;

    [Header("Showcase API")]
    [SerializeField] private ToxicologistPoisonCloudAbility poisonCloudAbility;
    [SerializeField] private RangerProjectileFireController rangerFire;

    [Header("HUD")]
    [Tooltip(
        "Controls which class HUD is visible during the automated showcase."
    )]
    [SerializeField] private ClassHUDManager classHUDManager;

    [Header("Enemy Setup")]
    [SerializeField] private GameObject easyEnemyPrefab;
    [SerializeField] private int enemyCount = 6;
    [SerializeField] private float enemyDistanceInFront = 5f;
    [SerializeField] private float enemySpacing = 1.1f;

    [Tooltip(
        "Raises the enemy prefab roots above the ground. " +
        "Increase this if enemies appear buried."
    )]
    [SerializeField] private float enemyHeightOffset = 1f;

    [Header("Player Setup")]
    [SerializeField] private float playerDistanceBehindCloud = 5f;

    [Tooltip(
        "Raises the active player root above the ground for this showcase."
    )]
    [SerializeField] private float playerHeightOffset = 1f;

    [Header("Reaction Placement")]
    [Tooltip(
        "Raises the poison cloud and reaction VFX above the ground."
    )]
    [SerializeField] private float cloudHeightOffset = 1f;

    [Tooltip(
        "Vertical point the Ranger aims toward."
    )]
    [SerializeField] private float shotAimHeightOffset = 1f;

    [Header("Timing")]
    [SerializeField] private float openingPause = 0.5f;
    [SerializeField] private float cloudHoldTime = 0.75f;
    [SerializeField] private float afterSwitchPause = 0.3f;
    [SerializeField] private float afterExplosionPause = 4f;

    [Header("Debug")]
    [SerializeField] private bool logSteps = true;

    private bool isRunning;
    private GameObject[] spawnedEnemies;

    private void Awake()
    {
        if (classHUDManager == null)
        {
            classHUDManager =
                FindAnyObjectByType<ClassHUDManager>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(startKey) && !isRunning)
        {
            StartCoroutine(
                RunVolatileReactionShowcase()
            );
        }
    }

    private IEnumerator RunVolatileReactionShowcase()
    {
        isRunning = true;

        Log("Showcase started.");

        // Match the working Meteor Dive showcase:
        // all action moves left-to-right in world space.
        Vector3 forward = Vector3.right;

        Quaternion forwardRotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );

        Vector3 referencePosition =
            toxicologistObject != null
                ? toxicologistObject.transform.position
                : transform.position;

        referencePosition.y = 0f;

        Vector3 horizontalCenter =
            referencePosition +
            forward * enemyDistanceInFront;

        horizontalCenter.y = 0f;

        Vector3 enemyCenter = horizontalCenter;
        enemyCenter.y = enemyHeightOffset;

        Vector3 cloudCenter = horizontalCenter;
        cloudCenter.y = cloudHeightOffset;

        Vector3 shotTarget = horizontalCenter;
        shotTarget.y = shotAimHeightOffset;

        Vector3 playerStartPosition =
            horizontalCenter -
            forward * playerDistanceBehindCloud;

        playerStartPosition.y =
            playerHeightOffset;

        Log(
            $"Positions calculated. " +
            $"Player={playerStartPosition}, " +
            $"Enemies={enemyCenter}, " +
            $"Cloud={cloudCenter}, " +
            $"ShotTarget={shotTarget}."
        );

        SpawnEnemyCluster(
            enemyCenter,
            Quaternion.LookRotation(
                -forward,
                Vector3.up
            )
        );

        ActivateToxicologist(
            playerStartPosition,
            forwardRotation
        );

        yield return new WaitForSeconds(
            openingPause
        );

        Log(
            $"Deploying Poison Cloud at {cloudCenter}."
        );

        bool cloudSpawned =
            poisonCloudAbility != null &&
            poisonCloudAbility
                .ForceSpawnCloudForShowcase(
                    cloudCenter
                );

        if (!cloudSpawned)
        {
            Debug.LogWarning(
                "[VolatileReactionShowcaseDirector] " +
                "Poison Cloud failed to spawn."
            );

            isRunning = false;
            yield break;
        }

        yield return new WaitForSeconds(
            cloudHoldTime
        );

        Log("Switching to Ranger.");

        Vector3 toxicologistFinalPosition =
            toxicologistObject != null
                ? toxicologistObject.transform.position
                : playerStartPosition;

        ActivateRanger(
            toxicologistFinalPosition,
            forwardRotation
        );

        yield return new WaitForSeconds(
            afterSwitchPause
        );

        Log(
            $"Firing Explosive Shot at {shotTarget}."
        );

        bool fired =
            rangerFire != null &&
            rangerFire
                .ForceExplosiveFireForShowcase(
                    shotTarget
                );

        if (!fired)
        {
            Debug.LogWarning(
                "[VolatileReactionShowcaseDirector] " +
                "Explosive Shot failed to fire."
            );
        }

        yield return new WaitForSeconds(
            afterExplosionPause
        );

        Log("Showcase complete.");

        isRunning = false;
    }

    private void ActivateToxicologist(
        Vector3 position,
        Quaternion rotation
    )
    {
        if (guardianObject != null)
            guardianObject.SetActive(false);

        if (rangerObject != null)
            rangerObject.SetActive(false);

        if (toxicologistObject != null)
        {
            toxicologistObject.SetActive(true);

            SetCharacterTransform(
                toxicologistObject,
                position,
                rotation
            );
        }

        ShowClassHUD(PlayerClassType.Healer);
    }

    private void ActivateRanger(
        Vector3 position,
        Quaternion rotation
    )
    {
        if (guardianObject != null)
            guardianObject.SetActive(false);

        if (toxicologistObject != null)
            toxicologistObject.SetActive(false);

        if (rangerObject != null)
        {
            rangerObject.SetActive(true);

            SetCharacterTransform(
                rangerObject,
                position,
                rotation
            );
        }

        ShowClassHUD(PlayerClassType.DPS);
    }

    private void ShowClassHUD(
        PlayerClassType classType
    )
    {
        if (classHUDManager == null)
        {
            classHUDManager =
                FindAnyObjectByType<ClassHUDManager>();
        }

        if (classHUDManager == null)
        {
            Debug.LogWarning(
                "[VolatileReactionShowcaseDirector] " +
                "Missing ClassHUDManager reference."
            );

            return;
        }

        classHUDManager.ShowHUD(classType);
    }

    private void SetCharacterTransform(
        GameObject character,
        Vector3 position,
        Quaternion rotation
    )
    {
        if (character == null)
            return;

        CharacterController controller =
            character.GetComponent<
                CharacterController
            >();

        bool controllerWasEnabled =
            controller != null &&
            controller.enabled;

        if (controllerWasEnabled)
            controller.enabled = false;

        character.transform.position =
            position;

        character.transform.rotation =
            rotation;

        if (controllerWasEnabled)
            controller.enabled = true;

        Physics.SyncTransforms();
    }

    private void SpawnEnemyCluster(
        Vector3 center,
        Quaternion rotation
    )
    {
        ClearSpawnedEnemies();

        if (easyEnemyPrefab == null)
        {
            Debug.LogWarning(
                "[VolatileReactionShowcaseDirector] " +
                "Missing Easy Enemy Prefab."
            );

            return;
        }

        spawnedEnemies =
            new GameObject[enemyCount];

        for (int i = 0; i < enemyCount; i++)
        {
            int row = i / 3;
            int column = i % 3;

            Vector3 offset = new Vector3(
                (column - 1) * enemySpacing,
                0f,
                row * enemySpacing
            );

            Vector3 spawnPosition =
                center + offset;

            spawnPosition.y =
                enemyHeightOffset;

            GameObject enemy = Instantiate(
                easyEnemyPrefab,
                spawnPosition,
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

    private void DisableEnemyMovement(
        GameObject enemy
    )
    {
        if (enemy == null)
            return;

        MonoBehaviour[] behaviours =
            enemy.GetComponentsInChildren<
                MonoBehaviour
            >();

        foreach (
            MonoBehaviour behaviour in behaviours
        )
        {
            if (behaviour == null)
                continue;

            string typeName =
                behaviour.GetType().Name;

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
        if (!logSteps)
            return;

        Debug.Log(
            "[VolatileReactionShowcaseDirector] " +
            message
        );
    }
}