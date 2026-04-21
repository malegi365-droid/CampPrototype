using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyGroupTemplate", menuName = "CampPrototype/Enemy Group Template")]
public class EnemyGroupTemplate : ScriptableObject
{
    public string groupName = "New Group";
    public List<EnemySpawnEntry> entries = new List<EnemySpawnEntry>();
}