using UnityEngine;
using System.Collections;

public class PlayerDebuffManager : MonoBehaviour
{
    public static PlayerDebuffManager Instance { get; private set; }

    [Header("Slow")]
    [SerializeField] float slowMultiplier = 0.4f;

    public bool IsSlowed { get; private set; }
    public bool IsConfused { get; private set; }
    public float SlowMultiplier => slowMultiplier;

    Coroutine slowCoroutine;
    Coroutine confuseCoroutine;

    void Awake() => Instance = this;

    public void ApplySlow(float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowTimer(duration));
    }

    public void ApplyConfusion(float duration)
    {
        if (confuseCoroutine != null) StopCoroutine(confuseCoroutine);
        confuseCoroutine = StartCoroutine(ConfuseTimer(duration));
    }

    public void ApplyBoth(float duration)
    {
        ApplySlow(duration);
        ApplyConfusion(duration);
    }

    IEnumerator SlowTimer(float duration)
    {
        IsSlowed = true;
        yield return new WaitForSeconds(duration);
        IsSlowed = false;
    }

    IEnumerator ConfuseTimer(float duration)
    {
        IsConfused = true;
        yield return new WaitForSeconds(duration);
        IsConfused = false;
    }
}
