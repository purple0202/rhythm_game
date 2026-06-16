using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeaponEntry
{
    public Weapon weapon;
    public int fmodValue;
}

[System.Serializable]
public class WeaponColourGroup
{
    public string groupId;
    public WeaponEntry[] weapons;
}

public class WeaponController : MonoBehaviour
{
    public static event System.Action OnFirstWeaponEquipped;
    public static event System.Action<int> OnWeaponSwitched;
    public static event System.Action<float, EnemyType> OnAttackFired;

    public WeaponUI weaponUI;

    [Header("First Weapon (fixed — red box drop)")]
    public Weapon firstWeapon;

    [Header("Colour Groups")]
    public WeaponColourGroup[] colourGroups;

    private List<Weapon> equippedWeapons = new List<Weapon>();
    public IReadOnlyList<Weapon> EquippedWeapons => equippedWeapons;

    // uiWeaponIndex  — what is highlighted in the UI right now (changes immediately).
    // activeWeaponIndex — which weapon's GameObject is actually SetActive(true).
    // pendingWeaponIndex — queued switch that waits for the active weapon to finish animating.
    private int uiWeaponIndex = -1;
    private int activeWeaponIndex = -1;
    private int pendingWeaponIndex = -1;

    void Awake()
    {
        DisableAll(firstWeapon);
        if (colourGroups != null)
            foreach (var group in colourGroups)
                DisableGroup(group?.weapons);
    }

    void DisableAll(Weapon w) { if (w != null) w.gameObject.SetActive(false); }
    void DisableGroup(WeaponEntry[] group) { if (group == null) return; foreach (var e in group) DisableAll(e.weapon); }

    void Update()
    {
        if (equippedWeapons.Count == 0 || uiWeaponIndex < 0) return;

        // Commit a deferred switch as soon as the active weapon finishes animating.
        if (pendingWeaponIndex != -1 && !equippedWeapons[activeWeaponIndex].IsAttacking)
            CommitSwitch(pendingWeaponIndex);

        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedWeapons.Count >= 1) RequestSwitch(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && equippedWeapons.Count >= 2) RequestSwitch(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && equippedWeapons.Count >= 3) RequestSwitch(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) && equippedWeapons.Count >= 4) RequestSwitch(3);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)      RequestSwitch(PrevNonPassive(uiWeaponIndex));
        else if (scroll < 0f) RequestSwitch(NextNonPassive(uiWeaponIndex));
    }

    int NextNonPassive(int from)
    {
        int count = equippedWeapons.Count;
        for (int i = 1; i < count; i++)
        {
            int idx = (from + i) % count;
            if (!equippedWeapons[idx].IsPassive) return idx;
        }
        return from;
    }

    int PrevNonPassive(int from)
    {
        int count = equippedWeapons.Count;
        for (int i = 1; i < count; i++)
        {
            int idx = (from - i + count) % count;
            if (!equippedWeapons[idx].IsPassive) return idx;
        }
        return from;
    }

    // UI highlights the new weapon immediately; the actual GameObject swap is
    // deferred if the current weapon is mid-animation.
    void RequestSwitch(int index)
    {
        if (index < 0 || index >= equippedWeapons.Count) return;
        if (equippedWeapons[index].IsPassive) return;
        if (index == uiWeaponIndex) return;

        uiWeaponIndex = index;
        weaponUI?.RefreshUI(equippedWeapons, uiWeaponIndex);

        if (activeWeaponIndex < 0 || !equippedWeapons[activeWeaponIndex].IsAttacking)
            CommitSwitch(index);
        else
            pendingWeaponIndex = index;
    }

    // Does the actual SetActive swap and clears the pending flag.
    void CommitSwitch(int index)
    {
        if (index == activeWeaponIndex) { pendingWeaponIndex = -1; return; }

        if (activeWeaponIndex >= 0 && activeWeaponIndex < equippedWeapons.Count
            && !equippedWeapons[activeWeaponIndex].IsPassive)
            equippedWeapons[activeWeaponIndex].gameObject.SetActive(false);

        activeWeaponIndex = index;
        pendingWeaponIndex = -1;
        equippedWeapons[activeWeaponIndex].gameObject.SetActive(true);
        OnWeaponSwitched?.Invoke(activeWeaponIndex);
    }

    // Called by WeaponDropData (red box) — equips firstWeapon directly, no UI
    public void EquipDirect()
    {
        if (equippedWeapons.Count > 0 || firstWeapon == null) return;

        equippedWeapons.Add(firstWeapon);
        uiWeaponIndex = 0;
        CommitSwitch(0);
        OnFirstWeaponEquipped?.Invoke();

        BeatConductor.Instance.SetParameter("Tutorial Success", 1f);
        BeatConductor.Instance.SetParameter("Synth 4", 1f);
        BeatConductor.Instance.SetParameter("Synth 5", 1f);

        weaponUI?.RefreshUI(equippedWeapons, uiWeaponIndex);
    }

    // Called by WeaponGroupData (coloured box) — opens selection UI for the matching colour group
    public void OpenGroupSelect(string groupId)
    {
        int nextSlot = equippedWeapons.Count;

        string fmodParam = nextSlot switch
        {
            1 => "Group 2",
            2 => "Group 3",
            3 => "Group 4",
            _ => ""
        };

        if (string.IsNullOrEmpty(fmodParam) || colourGroups == null) return;

        WeaponColourGroup group = System.Array.Find(colourGroups, g => g.groupId == groupId);
        if (group == null || group.weapons == null || group.weapons.Length == 0) return;

        Weapon[] weapons = System.Array.ConvertAll(group.weapons, e => e.weapon);
        int[] fmodValues = System.Array.ConvertAll(group.weapons, e => e.fmodValue);

        Time.timeScale = 0f;
        WeaponSelectUI.Instance.Show(weapons, fmodParam, fmodValues, this);
    }

    // Called by WeaponSelectUI after the player picks an option
    public void EquipGroupWeapon(Weapon weapon, string fmodParam, int fmodValue)
    {
        int slotIndex = equippedWeapons.Count;
        equippedWeapons.Add(weapon);

        if (slotIndex == 1)
        {
            BeatConductor.Instance.SetParameter("Weapon 1", 1f);
            BeatConductor.Instance.SetParameter("Synth 2", 1f);
        }

        if (slotIndex == 2)
        {
            BeatConductor.Instance.SetParameter("Synth 3", 1f);
        }

        BeatConductor.Instance.SetParameter(fmodParam, (float)fmodValue);

        if (weapon.IsPassive)
            weapon.gameObject.SetActive(true);

        weaponUI?.RefreshUI(equippedWeapons, uiWeaponIndex);
    }

    public void PerformAttack(string judgement)
    {
        if (activeWeaponIndex < 0 || activeWeaponIndex >= equippedWeapons.Count) return;

        Weapon weapon = equippedWeapons[activeWeaponIndex];

        if (judgement == "Auto" && JohnCageSilenceEffect.IsActive)
        {
            JohnCageSilenceEffect.Instance.AccumulateDamage(weapon.GetAutoDamage());
            return;
        }

        OnAttackFired?.Invoke(weapon.GetDamageForJudgement(judgement), weapon.weaponType);
        weapon.PerformAttack(judgement);

        foreach (var w in equippedWeapons)
            if (w.IsPassive) w.PerformAttack(judgement);

        if (PassiveManager.Instance != null)
            PassiveManager.Instance.PendingAttackBonus = 0f;
    }

    // Called by UnisonEffect on every Nth active hit — briefly activates each
    // non-active weapon, fires it, then deactivates it once its animation finishes.
    public void PerformUnisonAttack(string judgement)
    {
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (i == activeWeaponIndex) continue;
            StartCoroutine(UnisonFireWeapon(equippedWeapons[i], judgement));
        }
    }

    System.Collections.IEnumerator UnisonFireWeapon(Weapon weapon, string judgement)
    {
        weapon.gameObject.SetActive(true);
        weapon.PerformAttack(judgement);
        yield return new UnityEngine.WaitWhile(() => weapon.IsAttacking);
        if (equippedWeapons.IndexOf(weapon) != activeWeaponIndex)
            weapon.gameObject.SetActive(false);
    }

}
