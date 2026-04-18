using System.Collections.Generic;
using UnityEngine;

public class ThreatTable : MonoBehaviour
{
    private readonly Dictionary<GameObject, float> threatMap = new Dictionary<GameObject, float>();

    public void AddThreat(GameObject source, float amount)
    {
        if (source == null) return;

        if (!threatMap.ContainsKey(source))
        {
            threatMap[source] = 0f;
        }

        threatMap[source] += amount;
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

    public void ClearAllThreat()
    {
        threatMap.Clear();
    }
}