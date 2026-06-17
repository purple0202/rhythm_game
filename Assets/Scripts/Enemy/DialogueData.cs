using UnityEngine;

[CreateAssetMenu(menuName = "Game/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string speakerName;
    [TextArea] public string[] lines;
}
