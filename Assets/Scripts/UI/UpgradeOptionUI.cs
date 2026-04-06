using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionUI : MonoBehaviour
{
    public Text nameText;
    public Text descriptionText;
    public Image icon;
    public Button button;

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

    void OnClick()
    {
        Debug.Log("Button Clicked!");
        UpgradeUI.Instance.SelectUpgrade(currentUpgrade);
    }
}