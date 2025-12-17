using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerBall : MonoBehaviour
{
    [Header("Shoot Settings")]
    public float shootPower = 6f;     // 発射力
    public float maxScale = 0.1f;       // 最大スケール

    [Header("Physics Settings")]
    public float friction = 0.995f;   // 摩擦係数
    public float minVelocity = 0.03f; // 最小速度

    [Header("Scale Settings")]
    public float scaleReturnSpeed = 5f; // スケール復元速度


    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isDragging = false;
    private Vector3 targetScale = Vector3.one / 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void Update()
    {
        // 드래그 시작
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            rb.velocity = Vector2.zero;
        }

        // 드래그 중
        if (isDragging)
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - currentPos;
            float dragPower = dir.magnitude;

            // 공 크기 변화
            float scale =  Mathf.Clamp(dragPower / 2f, 0f, maxScale - 1f);
            targetScale = Vector3.one * 0.1f;

            rb.velocity = Vector2.zero;
        }

        // 드래그 종료 → 발사
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - endPos;

            rb.velocity = dir.normalized * dir.magnitude * shootPower;
            targetScale = Vector3.one / 20f;
            isDragging = false;
        }

        // 크기 자연 복원
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleReturnSpeed);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            StopImmediately();
        }
    }

    void StopImmediately()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
