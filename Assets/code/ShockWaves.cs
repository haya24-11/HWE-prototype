using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [Header("Shoot Settings")]
    public float shootPower = 6f;      // 発射の強さ
    public float maxScale = 2f;        // 最大スケール

    [Header("Physics Settings")]
    public float friction = 0.995f;    // 摩擦係数
    public float minVelocity = 0.03f;  // 最低速度

    [Header("Scale Settings")]
    public float scaleReturnSpeed = 5f; // スケール復元速度

    [Header("ShockWave Settings")]
    public GameObject shockWavePrefab; // 衝撃波のプレハブ
    public float shockWaveCooldown = 1.0f;

    private float lastShockWaveTime = 0f;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isDragging = false;
    private Vector3 targetScale = Vector3.one / 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;   // 重力を無効化
        rb.drag = 0f;           // 空気抵抗なし
        rb.angularDrag = 0f;    // 回転抵抗なし
        rb.freezeRotation = true;
    }

    void Update()
    {
        // ドラッグ開始
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            rb.velocity = Vector2.zero; // 一時停止
        }

        // ドラッグ中
        if (isDragging)
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - currentPos;
            float dragPower = dir.magnitude;

            // ボールのスケール変化
            float scale = 1f + Mathf.Clamp(dragPower / 2f, 0f, maxScale - 1f);
            targetScale = Vector3.one * scale;

            rb.velocity = Vector2.zero;
        }

        // ドラッグ終了 → 発射
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = startPos - endPos;

            rb.velocity = dir.normalized * dir.magnitude * shootPower;

            targetScale = Vector3.one / 2f;
            isDragging = false;
        }

        // スケールを自然に元に戻す
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleReturnSpeed
        );

        // スペースキーで衝撃波
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastShockWaveTime >= shockWaveCooldown)
            {
                SpawnShockWave();
                lastShockWaveTime = Time.time;
            }
        }
    }

    void SpawnShockWave()
    {
        if (shockWavePrefab != null)
        {
            Instantiate(shockWavePrefab, transform.position, Quaternion.identity);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }
}
