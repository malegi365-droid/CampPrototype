using System.Collections.Generic;
using UnityEngine;

public class ThreatTable : MonoBehaviour
{
    private readonly Dictionary<GameObject, float> threatMap = new Dictionary<GameObject, float>();

    public void AddThreat(GameObject source, float amount)
    {
        if (source == null) return;
        if (amount <= 0f) return;

        if (!threatMap.ContainsKey(source))
        {
            threatMap[source] = 0f;
        }

        threatMap[source] += amount;
    }

    public void ReduceThreat(GameObject source, float amount)
    {
        if (source == null) return;
        if (amount <= 0f) return;
        if (!threatMap.ContainsKey(source)) return;

        threatMap[source] -= amount;

        if (threatMap[source] <= 0f)
        {
            threatMap.Remove(source);
        }
    }

    public GameObject GetHighestThreatTarget()
    {
        float highestThreat = float.MinValue;
        GameObject bestTarget = null;

        List<GameObject> toRemove = new List<GameObject>();

        foreach (var pair in threatMap)
        {
            if (pair.Key == null || !pair.Key.activeInHierarchy)
            {
                toRemove.Add(pair.Key);
                continue;
            }

            if (pair.Value > highestThreat)
            {
                highestThreat = pair.Value;
                bestTarget = pair.Key;
            }
        }

        foreach (GameObject obj in toRemove)
        {
            threatMap.Remove(obj);
        }

        return bestTarget;
    }

    public float GetThreatFor(GameObject source)
    {
        if (source == null) return 0f;
        if (!threatMap.ContainsKey(source)) return 0f;

        return threatMap[source];
    }

    public bool HasThreatFor(GameObject source)
    {
        if (source == null) return false;
        return threatMap.ContainsKey(source);
    }

    public void ClearAllThreat()
    {
        threatMap.Clear();
    }
}