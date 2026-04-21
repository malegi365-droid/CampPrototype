using System;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;
    public int count = 1;
    public float spacing = 1.5f;
}