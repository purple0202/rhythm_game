using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    public Sprite icon;

    public abstract void PerformAttack(string judgement);
}