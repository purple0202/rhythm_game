using UnityEngine;

using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int count;
    public EnemyType enemyType = EnemyType.None;
}