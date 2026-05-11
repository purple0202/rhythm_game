using UnityEngine;

[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public bool enabled = true;
}
