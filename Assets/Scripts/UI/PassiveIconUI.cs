using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassiveIconUI : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] GameObject stackContainer;
    [SerializeField] TextMeshProUGUI stackText;
    [SerializeField] RectTransform stackPunchTarget;

    [Header("Colors")]
    [SerializeField] Color activeColor   = Color.white;
    [SerializeField] Color inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Punch")]
    [SerializeField] float punchScale    = 1.35f;
    [SerializeField] float punchDuration = 0.1f;

    public void Setup(PassiveData data)
    {
        iconImage.sprite = data.icon;
        iconImage.color  = inactiveColor;
        stackContainer.SetActive(false);
    }

    public void SetActive(bool active)
    {
        iconImage.color = active ? activeColor : inactiveColor;
    }

    public void SetStackCount(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            stackContainer.SetActive(false);
            return;
        }

        stackContainer.SetActive(true);
        stackText.text = text;
        StopAllCoroutines();
        StartCoroutine(PunchScale());
    }

    IEnumerator PunchScale()
    {
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchDuration;
            stackPunchTarget.localScale = Vector3.one * Mathf.Lerp(punchScale, 1f, t);
            yield return null;
        }
        stackPunchTarget.localScale = Vector3.one;
    }
}
