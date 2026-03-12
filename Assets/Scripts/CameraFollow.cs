using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;             // The player to follow
    public SpriteRenderer background;    // The background sprite

    [Header("Settings")]
    public float smoothSpeed = 5f;       // Camera follow smoothing

    private float camHalfWidth;
    private float camHalfHeight;

    private float minX, maxX, minY, maxY;

    void Start()
    {
        if (player == null) Debug.LogError("Player not assigned in CameraFollow!");
        if (background == null) Debug.LogError("Background not assigned in CameraFollow!");

        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // Calculate automatic bounds from background sprite
        Vector3 bgPos = background.transform.position;
        Vector2 bgSize = background.bounds.size;  // Full size in world units

        minX = bgPos.x - bgSize.x / 2f;
        maxX = bgPos.x + bgSize.x / 2f;
        minY = bgPos.y - bgSize.y / 2f;
        maxY = bgPos.y + bgSize.y / 2f;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Target camera position follows player
        float targetX = Mathf.Clamp(player.position.x, minX + camHalfWidth, maxX - camHalfWidth);
        float targetY = Mathf.Clamp(player.position.y, minY + camHalfHeight, maxY - camHalfHeight);

        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}