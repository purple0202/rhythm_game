using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WeaponDotSelectUI : MonoBehaviour
{
    public static WeaponDotSelectUI Instance;

    public GameObject panel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private DotType pendingDot;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(DotType dot, IReadOnlyList<Weapon> weapons)
    {
        pendingDot = dot;

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (Weapon weapon in weapons)
        {
            Weapon captured = weapon;
            GameObject btn = Instantiate(buttonPrefab, buttonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = weapon.name;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectWeapon(captured));
        }

        panel.SetActive(true);
    }

    void SelectWeapon(Weapon weapon)
    {
        if (!weapon.dotApplications.Contains(pendingDot))
            weapon.dotApplications.Add(pendingDot);

        panel.SetActive(false);
        UpgradeUI.Instance.ResumeAfterSelect();
    }
}
