using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    public Sprite icon;
    public EnemyType weaponType = EnemyType.None;

    public abstract void PerformAttack(string judgement);
}