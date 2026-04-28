using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponSelectUI : MonoBehaviour
{
    public static WeaponSelectUI Instance;

    public GameObject panel;
    public WeaponOptionUI[] optionSlots;

    WeaponController pendingController;
    string pendingFmodParam;
    int selectedIndex;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(Weapon[] weapons, string fmodParam, WeaponController controller)
    {
        pendingController = controller;
        pendingFmodParam = fmodParam;

        for (int i = 0; i < optionSlots.Length; i++)
            optionSlots[i].Setup(weapons[i], i);

        selectedIndex = 0;
        panel.SetActive(true);
        Time.timeScale = 0f;
        FocusSelected();
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            Navigate(1);

        if (Input.GetKeyDown(KeyCode.P))
            optionSlots[selectedIndex].Select();
    }

    public void SetSelected(int index)
    {
        optionSlots[selectedIndex].SetHighlighted(false);
        selectedIndex = index;
        FocusSelected();
    }

    void Navigate(int dir)
    {
        SetSelected((selectedIndex + dir + optionSlots.Length) % optionSlots.Length);
    }

    void FocusSelected()
    {
        foreach (var slot in optionSlots)
            slot.SetHighlighted(false);
        EventSystem.current.SetSelectedGameObject(optionSlots[selectedIndex].button.gameObject);
        optionSlots[selectedIndex].SetHighlighted(true);
    }

    // Called by WeaponOptionUI — optionIndex is 0-based, FMOD value is 1-based
    public void SelectWeapon(Weapon weapon, int optionIndex)
    {
        pendingController.EquipGroupWeapon(weapon, pendingFmodParam, optionIndex + 1);
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
