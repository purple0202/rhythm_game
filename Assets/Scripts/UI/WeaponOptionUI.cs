using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponOptionUI : MonoBehaviour, IPointerEnterHandler
{
    public Text nameText;
    public Text descriptionText;
    public Image icon;
    public Button button;
    public GameObject selectionBorder;

    Weapon currentWeapon;
    int myIndex;

    public void Setup(Weapon weapon, int index)
    {
        currentWeapon = weapon;
        myIndex = index;

        nameText.text = weapon.weaponName;
        descriptionText.text = weapon.description;
        icon.sprite = weapon.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        WeaponSelectUI.Instance.SetSelected(myIndex);
    }

    public void SetHighlighted(bool on)
    {
        if (selectionBorder != null) selectionBorder.SetActive(on);
    }

    public void Select() => WeaponSelectUI.Instance.SelectWeapon(currentWeapon, myIndex);

    void OnClick() => Select();
}
