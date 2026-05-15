using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TaperedText : MonoBehaviour
{
    // 1 = full character height, 0.5 = half height
    public float leftScale   = 1f;
    public float rightScale  = 0.5f;
    // flat pixel boost added to each character's half-height after scaling (0 = no effect)
    public float heightBonus = 0f;

    TextMeshProUGUI tmp;

    void Awake() => tmp = GetComponent<TextMeshProUGUI>();

    void LateUpdate() => Apply();

    public void Apply()
    {
        if (tmp == null) return;

        tmp.ForceMeshUpdate();
        var textInfo = tmp.textInfo;
        if (textInfo.characterCount == 0) return;

        // Find the horizontal extent of the visible text
        float textLeft  = float.MaxValue;
        float textRight = float.MinValue;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var c = textInfo.characterInfo[i];
            if (!c.isVisible) continue;
            if (c.origin   < textLeft)  textLeft  = c.origin;
            if (c.xAdvance > textRight) textRight = c.xAdvance;
        }

        float textWidth = textRight - textLeft;
        if (textWidth <= 0f) return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var c = textInfo.characterInfo[i];
            if (!c.isVisible) continue;

            var meshInfo = textInfo.meshInfo[c.materialReferenceIndex];
            int vi = c.vertexIndex;

            // t=0 at left edge of text, t=1 at right edge
            float t = (c.origin - textLeft) / textWidth;
            float scale = Mathf.Lerp(leftScale, rightScale, t);

            // TMP vertex order per quad: BL(0) TL(1) TR(2) BR(3)
            float centerY = (meshInfo.vertices[vi].y + meshInfo.vertices[vi + 2].y) * 0.5f;

            for (int j = 0; j < 4; j++)
            {
                float dist = meshInfo.vertices[vi + j].y - centerY;
                meshInfo.vertices[vi + j].y = centerY + dist * scale + Mathf.Sign(dist) * heightBonus;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var mesh = textInfo.meshInfo[i].mesh;
            mesh.vertices = textInfo.meshInfo[i].vertices;
            tmp.UpdateGeometry(mesh, i);
        }
    }
}
