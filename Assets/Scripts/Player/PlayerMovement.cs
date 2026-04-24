using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int facingDirection = 1;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        //MOVE TO GAME MANAGER LATER
        //Physics2D.IgnoreLayerCollision(
        //    LayerMask.NameToLayer("Player"),
        //    LayerMask.NameToLayer("Enemy"),
        //    true
        //);

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

        if (movement.x != 0)
            facingDirection = movement.x > 0 ? 1 : -1;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }
}