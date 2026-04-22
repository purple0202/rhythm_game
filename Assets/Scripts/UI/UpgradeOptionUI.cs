using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public Text nameText;
    public Text descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    UpgradeData currentUpgrade;
    int myIndex;

    public void Setup(UpgradeData upgrade, int index)
    {
        currentUpgrade = upgrade;
        myIndex = index;

        nameText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        icon.sprite = upgrade.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => UpgradeUI.Instance.SelectUpgrade(currentUpgrade);

    void OnClick() => Select();
}