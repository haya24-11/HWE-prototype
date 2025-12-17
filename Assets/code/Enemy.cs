using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    private EnemySpawner spawner;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;

    //==============================
    // 移動設定
    //==============================
    [Header("Move Settings")]
    public float moveForce = 3f;     // プレイヤーに向かう力
    public float maxSpeed = 5f;      // 最大速度

    //==============================
    // 追跡停止距離
    //==============================
    [Header("Chase Settings")]
    public float stopDistance = 6.0f; // この距離以内で減速

    //==============================
    // バウンド制御
    //==============================
    [Header("Bounce Control")]
    public float stopChaseTime = 0.2f; // 反射後、追跡を止める時間
    private float lastBounceTime = -10f;

    //==============================
    // ヒット（HP）設定
    //==============================
    [Header("Hit Settings")]
    public int maxHits = 2;          // 最大ヒット数
    private int hitCount = 0;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.1f;

    //==============================
    // プレイヤーによって飛ばされた状態
    //==============================
    [Header("Launch State")]
    public float launchedTime = 0.3f; // 「飛ばされ状態」の有効時間
    private float launchedTimer = -10f;

    [Header("Launch Force Settings")]
    public float minLaunchSpeed = 3f; // 이 속도 이상일 때만 Launch 인정



    [Header("Damage Condition")]
    public float damageSpeedThreshold = 4.0f; // 이 속도 이상일 때만 데미지

    //==============================
    // 初期化
    //==============================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        rb.mass = 0.5f;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        // Playerを取得
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    //==============================
    // 追跡処理
    //==============================
    void FixedUpdate()
    {
        if (player == null) return;

        // 反射直後は追跡しない
        if (Time.time - lastBounceTime < stopChaseTime)
            return;

        Vector2 playerPos = (Vector2)player.position;
        float distance = Vector2.Distance(rb.position, playerPos);

        // 近すぎたら減速
        if (distance <= stopDistance)
        {
            rb.velocity *= 0.9f;
            return;
        }

        Vector2 dir = (playerPos - rb.position).normalized;
        rb.AddForce(dir * moveForce, ForceMode2D.Force);

        // 最大速度制限
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }
    public void LaunchByPlayer(Vector2 force)
    {
        rb.velocity = force;
        launchedTimer = Time.time;
        lastBounceTime = Time.time; // 즉시 추적 방지
    }

    public void SetSpawner(EnemySpawner s)
    {
        spawner = s;
    }

    //==============================
    // 衝突処理
    //==============================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ───── Playerに当たった場合 ─────
        if (collision.gameObject.CompareTag("Player"))
        {
            launchedTimer = Time.time; // ⭐ プレイヤーに飛ばされた
            Reflect(collision);
            AddHit();
            return;
        }

        // ───── Enemyに当たった場合 ─────
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy other = collision.gameObject.GetComponent<Enemy>();

            // ⭐ 플레이어에게 맞아 날아간 상태 + 충분한 속도일 때만 데미지
            if (IsLaunched() && rb.velocity.magnitude >= damageSpeedThreshold)
            {
                if (other != null)
                {
                    other.AddHit();
                }
            }

            Reflect(collision);
            return;
        }


        // ───── Wall ─────
        if (collision.gameObject.CompareTag("Wall"))
        {
            Reflect(collision);
        }
    }

    //==============================
    // 反射処理
    //==============================
    void Reflect(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;
        Vector2 normal = collision.contacts[0].normal;

        Vector2 otherVel = otherRb ? otherRb.velocity : Vector2.zero;
        Vector2 relativeVelocity = rb.velocity - otherVel;

        Vector2 reflect = Vector2.Reflect(relativeVelocity, normal);

        // ワールド基準で反射
        rb.velocity = reflect + otherVel;

        lastBounceTime = Time.time;
    }

    //==============================
    // ヒット判定
    //==============================
    bool IsLaunched()
    {
        return Time.time - launchedTimer < launchedTime;
    }

    void AddHit()
    {
        if (Time.time - lastHitTime < hitCooldown) return;

        lastHitTime = Time.time;
        hitCount++;

        // 色変更（ダメージ表現）
        if (sr != null)
        {
            if (hitCount == 1) sr.color = Color.blue;
            else if (hitCount == 2) sr.color = Color.red;
        }

        if (hitCount >= maxHits)
            Die();
    }

    //==============================
    // 死亡処理
    //==============================
    void Die()
    {
        if (spawner != null)
            spawner.OnEnemyDestroyed();

        Destroy(gameObject);
    }
}
