using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FixedWall : MonoBehaviour
{
    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static; // Rigidbody2Dを静的に設定（動かない壁用）
    }
}
