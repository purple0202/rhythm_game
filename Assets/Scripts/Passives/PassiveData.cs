using UnityEngine;

[CreateAssetMenu(menuName = "Game/Passive")]
public class PassiveData : ScriptableObject
{
    public string passiveName;
    [TextArea] public string description;
    public Sprite icon;
}
