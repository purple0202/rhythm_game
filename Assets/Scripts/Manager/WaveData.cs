using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave")]
public class WaveData : ScriptableObject
{
    public List<EnemySpawnData> enemies;
}