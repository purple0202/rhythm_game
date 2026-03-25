using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;

    public abstract void PerformAttack(string judgement);
}