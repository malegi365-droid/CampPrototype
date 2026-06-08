using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = new Color(0.75f, 1f, 1f, 1f);
    [SerializeField] private float flashDuration = 0.07f;
    [SerializeField] private float flashBlend = 0.65f;

    [Header("Renderers")]
    [SerializeField] private Renderer[] renderers;

    private Material[][] materials;
    private Color[][] originalColors;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        CacheMaterialsAndColors();
    }

    private void CacheMaterialsAndColors()
    {
        materials = new Material[renderers.Length][];
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            materials[i] = renderers[i].materials;
            originalColors[i] = new Color[materials[i].Length];

            for (int j = 0; j < materials[i].Length; j++)
            {
                originalColors[i][j] = materials[i][j].color;
            }
        }
    }

    public void TriggerFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    public void Flash()
    {
        TriggerFlash();
    }

    private IEnumerator FlashRoutine()
    {
        SetFlash(true);

        yield return new WaitForSeconds(flashDuration);

        SetFlash(false);

        flashRoutine = null;
    }

    private void SetFlash(bool active)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
                continue;

            for (int j = 0; j < materials[i].Length; j++)
            {
                if (materials[i][j] == null)
                    continue;

                materials[i][j].color = active
                    ? Color.Lerp(originalColors[i][j], flashColor, flashBlend)
                    : originalColors[i][j];
            }
        }
    }
}