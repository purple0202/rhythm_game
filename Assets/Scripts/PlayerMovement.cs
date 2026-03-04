using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("Rigidbody2D not found on " + gameObject.name);
        else
            Debug.Log("Rigidbody2D found: " + rb.name);
        //rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = movement * moveSpeed;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}