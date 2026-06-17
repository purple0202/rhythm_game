using UnityEngine;
using System.Collections;

public class PlayerLevelUpVFX : MonoBehaviour
{
    [SerializeField] GameObject vfxObject;
    [SerializeField] float vfxDuration = 1f;

    void Awake()
    {
        if (vfxObject != null) vfxObject.SetActive(false);
    }

    void OnEnable()  => LevelSystem.OnLevelUp += OnLevelUp;
    void OnDisable() => LevelSystem.OnLevelUp -= OnLevelUp;

    void OnLevelUp()
    {
        if (vfxObject == null) return;

        StopAllCoroutines();
        // Toggle off/on so the Animator resets to its default state and replays from frame 0.
        vfxObject.SetActive(false);
        vfxObject.SetActive(true);
        StartCoroutine(HideAfter(vfxDuration));
    }

    IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        vfxObject.SetActive(false);
    }
}
