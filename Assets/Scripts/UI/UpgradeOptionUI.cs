using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionUI : MonoBehaviour
{
    public Text nameText;
    public Text descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    UpgradeData currentUpgrade;

    public void Setup(UpgradeData upgrade)
    {
        currentUpgrade = upgrade;

        nameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        icon.sprite = upgrade.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => UpgradeUI.Instance.SelectUpgrade(currentUpgrade);

    void OnClick() => Select();
}