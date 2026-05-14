using UnityEngine;

public class BossIndicator : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] Sprite chargeSprite;
    [SerializeField] Sprite attackSprite;
    [SerializeField] Sprite chainLinkSprite;   // placed between consecutive attack beats

    [Header("Layout")]
    [SerializeField] float iconSpacing = 0.45f;
    [SerializeField] float iconScale = 0.25f;

    [Header("Colors")]
    public Color typeColor = Color.white;       // set per enemy type (blue/red/green/yellow)
    [SerializeField] Color chargeGlowColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] Color attackGlowColor = new Color(1f, 0.3f, 0.3f);

    [Header("Pulse")]
    [SerializeField] float pulseSpeed = 5f;
    [SerializeField] float pulseScaleBoost = 0.15f;
    [SerializeField] float dimAlpha = 0.35f;

    SpriteRenderer[] iconRenderers;
    GameObject[] iconObjects;
    bool[] isAttack;
    int currentBeat;
    int patternLen;
    int pulsingIndex;

    // Called on spawn and whenever the pattern changes
    public void Setup(int patternLength)
    {
        patternLen = patternLength;
        currentBeat = 0;
        pulsingIndex = 0;

        isAttack = new bool[patternLength];
        isAttack[patternLength - 1] = true;

        RebuildIcons();
    }

    // Called by MinibossController each beat, before the beat action fires
    public void OnBeat(int beatIndex, int patternLength)
    {
        currentBeat = beatIndex;
        pulsingIndex = (beatIndex + 1) % patternLength;
        RefreshIconColors();
    }

    void RebuildIcons()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        iconObjects = new GameObject[patternLen];
        iconRenderers = new SpriteRenderer[patternLen];

        float totalWidth = (patternLen - 1) * iconSpacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < patternLen; i++)
        {
            var go = new GameObject($"BeatIcon_{i}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(startX + i * iconSpacing, 0f, 0f);
            go.transform.localScale = Vector3.one * iconScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = isAttack[i] ? attackSprite : chargeSprite;
            sr.color = typeColor;

            iconObjects[i] = go;
            iconRenderers[i] = sr;
        }

        PlaceChainLinks(startX);
        RefreshIconColors();
    }

    void PlaceChainLinks(float startX)
    {
        if (chainLinkSprite == null) return;

        for (int i = 0; i < patternLen - 1; i++)
        {
            if (!isAttack[i] || !isAttack[i + 1]) continue;

            var link = new GameObject($"ChainLink_{i}");
            link.transform.SetParent(transform, false);
            link.transform.localPosition = new Vector3(startX + i * iconSpacing + iconSpacing * 0.5f, 0f, 0f);
            link.transform.localScale = Vector3.one * iconScale * 0.5f;

            var sr = link.AddComponent<SpriteRenderer>();
            sr.sprite = chainLinkSprite;
            sr.color = typeColor;
        }
    }

    void RefreshIconColors()
    {
        if (iconRenderers == null) return;

        for (int i = 0; i < iconRenderers.Length; i++)
        {
            if (iconRenderers[i] == null) continue;
            if (i == pulsingIndex) continue; // Update drives the pulsing icon

            Color c = (i == currentBeat && isAttack[i]) ? attackGlowColor : typeColor;
            c.a = (i == currentBeat) ? 1f : dimAlpha;
            iconRenderers[i].color = c;
        }
    }

    void Update()
    {
        if (iconRenderers == null) return;
        if (pulsingIndex < 0 || pulsingIndex >= iconRenderers.Length) return;
        if (iconObjects[pulsingIndex] == null || iconRenderers[pulsingIndex] == null) return;

        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        Color glowTarget = isAttack[pulsingIndex] ? attackGlowColor : chargeGlowColor;
        iconRenderers[pulsingIndex].color = Color.Lerp(typeColor, glowTarget, t);

        float scale = iconScale * (1f + t * pulseScaleBoost);
        iconObjects[pulsingIndex].transform.localScale = Vector3.one * scale;
    }
}
