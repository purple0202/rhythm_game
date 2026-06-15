using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [SerializeField] WeaponBoxData defaultData;
    WeaponBoxData data;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (defaultData != null) Initialize(defaultData);
    }

    public void Initialize(WeaponBoxData boxData)
    {
        data = boxData;
        if (anim != null && boxData.animatorController != null)
            anim.runtimeAnimatorController = boxData.animatorController;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || data == null) return;

        WeaponController controller = other.GetComponentInChildren<WeaponController>();
        if (controller == null) return;

        data.OnPickup(controller);
        Destroy(gameObject);
    }
}
