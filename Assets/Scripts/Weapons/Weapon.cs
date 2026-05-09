using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    [TextArea] public string description;
    public Sprite icon;
    public EnemyType weaponType = EnemyType.None;

    public abstract void PerformAttack(string judgement);

    public virtual float GetAutoDamage() => 0f;
    public virtual float GetDamageForJudgement(string judgement) => 0f;

    public virtual bool IsAttacking => false;
}