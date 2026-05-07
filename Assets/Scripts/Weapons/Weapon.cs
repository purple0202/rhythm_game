using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    [TextArea] public string description;
    public Sprite icon;
    public EnemyType weaponType = EnemyType.None;

    public abstract void PerformAttack(string judgement);

    // Subclasses override this to signal that an attack animation is still running.
    public virtual bool IsAttacking => false;
}