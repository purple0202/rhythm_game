using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PassiveOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    PassiveData currentPassive;
    int myIndex;

    public void Setup(PassiveData passive, int index)
    {
        currentPassive = passive;
        myIndex = index;
        nameText.text = passive.passiveName;
        descriptionText.text = passive.description;
        icon.sprite = passive.icon;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PassiveSelectionUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => PassiveSelectionUI.Instance.SelectPassive(currentPassive);

    void OnClick() => Select();
}
