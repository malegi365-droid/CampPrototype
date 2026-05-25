using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;

    [SerializeField] private float flashDuration = 0.08f;

    [Header("Renderers")]
    [SerializeField] private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColorID =
        Shader.PropertyToID("_BaseColor");

    private Color[][] originalColors;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        propertyBlock = new MaterialPropertyBlock();

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;

            originalColors[i] = new Color[mats.Length];

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty(BaseColorID))
                    originalColors[i][j] =
                        mats[j].GetColor(BaseColorID);
                else
                    originalColors[i][j] = Color.white;
            }
        }
    }

    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetColor(flashColor);

        yield return new WaitForSeconds(flashDuration);

        RestoreOriginalColors();
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty(BaseColorID))
                    mats[j].SetColor(BaseColorID, color);
            }
        }
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty(BaseColorID))
                {
                    mats[j].SetColor(
                        BaseColorID,
                        originalColors[i][j]
                    );
                }
            }
        }
    }
}