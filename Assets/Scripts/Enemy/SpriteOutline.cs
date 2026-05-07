using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    [Range(1f, 1.2f)]
    public float outlineScale = 1.05f;
    public Material silhouetteMaterial;

    private SpriteRenderer parentRenderer;
    private SpriteRenderer outlineRenderer;
    private Transform outlineTransform;
    private MaterialPropertyBlock _mpb;
    private static readonly int ColorProp = Shader.PropertyToID("_Color");

    void Awake()
    {
        parentRenderer = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();

        GameObject outlineObject = new GameObject("_Outline");
        outlineTransform = outlineObject.transform;
        outlineTransform.SetParent(transform);
        outlineTransform.localPosition = Vector3.zero;
        outlineTransform.localRotation = Quaternion.identity;
        outlineTransform.localScale = Vector3.one * outlineScale;

        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = parentRenderer.sprite;
        outlineRenderer.sortingLayerID = parentRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = parentRenderer.sortingOrder - 1;
        outlineRenderer.material = silhouetteMaterial;
    }

    void LateUpdate()
    {
        outlineRenderer.sprite = parentRenderer.sprite;
        outlineRenderer.sortingOrder = parentRenderer.sortingOrder - 1;

        float x = parentRenderer.flipX ? -outlineScale : outlineScale;
        float y = parentRenderer.flipY ? -outlineScale : outlineScale;
        outlineTransform.localScale = new Vector3(x, y, 1f);
    }

    public void SetColor(Color color)
    {
        if (outlineRenderer == null) return;
        _mpb.SetColor(ColorProp, color);
        outlineRenderer.SetPropertyBlock(_mpb);
    }
}
