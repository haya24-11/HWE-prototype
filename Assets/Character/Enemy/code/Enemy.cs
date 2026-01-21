using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    private Vector2 prevVelocity;

    private EnemySpawner spawner;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;

    [Header("Move Settings")]
    public float moveForce = 3f;
    public float maxSpeed = 5f;

    [Header("Chase Settings")]
    public float stopDistance = 3.0f;

    [Header("Bounce Control")]
    public float stopChaseTime = 0.2f;
    private float lastBounceTime = -5f;

    [Header("HP Settings")]
    public int maxHP = 4;
    public int currentHP;

    private float lastHitTime = 0f;
    public float hitCooldown = 0.1f;

    [Header("Hit Effect")]
    public float hitFlashTime = 0.1f;
    private Color originalColor;
    private Coroutine hitCoroutine;

    [Header("Launch State")]
    public float launchedTime = 0.3f;
    private float launchedTimer = -10f;

    [Header("Damage Condition")]
    public float damageSpeedThreshold = 2.0f;

    [Header("Rewards")]
    public int xpReward = 3;

    // ✅ 핀볼 느낌(범퍼 킥) 파라미터
    [Header("Pinball Bounce")]
    public bool pinballEnabled = true;
    public float pinballMaxSpeed = 18f;

    [Header("Enemy Collision Damping")]
    [Range(0.1f, 1f)]
    public float enemyCollisionDamping = 0.5f;

    [Header("Separation")]
    public float separationRadius = 1.2f;
    public float separationForce = 5f;

    [Header("Boss")]
    public bool isBoss = false;

    [Header("Boss Move")]
    public float bossMoveSpeed = 2.5f;

    private Vector2 bossDesiredVelocity;

    [Header("Boss Stun")]
    public float bossStunTime = 0.3f;
    private float bossStunEndTime = -10f;


    private bool isDying = false;

    public IEnumerator DelayedDie(float delay)
    {
        if (isDying) yield break;
        isDying = true;

        // 디버그로 확인
        Debug.Log($"{name} will die after {delay}s");

        yield return new WaitForSeconds(delay);

        Die();
    }



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        rb.mass = 0.5f;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // 핀볼 느낌엔 드래그 거의 없는게 좋음
        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHP = maxHP;

        if (sr != null)
            originalColor = sr.color;

        if (isBoss)
            rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        ApplySeparation();

        if (player == null) return;

        // ===============================
        // ボス移動
        // ===============================
        if (isBoss)
        {
            // 硬直中は移動しない
            if (Time.time < bossStunEndTime)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.velocity = dir * bossMoveSpeed;
            return;
        }

        // ===============================
        // 一般モンスター移動
        // ===============================
        if (Time.time - lastBounceTime < stopChaseTime) return;

        Vector2 playerPos = player.position;
        float distance = Vector2.Distance(rb.position, playerPos);

        if (distance <= stopDistance)
        {
            rb.velocity *= 0.9f;
            return;
        }

        Vector2 moveDir = (playerPos - rb.position).normalized;
        rb.AddForce(moveDir * moveForce, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }



    /* =========================
       Launch 関連
       ========================= */
    public void LaunchByPlayer(Vector2 force)
    {
        rb.velocity = force;
        launchedTimer = Time.time;
        lastBounceTime = Time.time;
    }

    public void LaunchByEnemy(Vector2 force)
    {
        rb.velocity = force;
        launchedTimer = Time.time;
        lastBounceTime = Time.time;
    }

    public bool IsLaunched() => Time.time - launchedTimer < launchedTime;

    /* =========================
       衝突処理
       ========================= */
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy")) return;
        if (collision.contactCount == 0) return;

        Enemy other = collision.gameObject.GetComponent<Enemy>();
        if (other == null) return;

        // ===============================
        // 両方とも Launch 状態でなければ
        // ボスと衝突しても何も起こらない
        // ===============================
        if (!IsLaunched() && !other.IsLaunched())
            return;
            }
        }

        // ===============================
        // Launch 状態の通常モンスター ↔ ボス
        // ===============================

        // 自分がボス ＆ 相手が通常モンスター ＆ 相手が Launch 状態
        if (isBoss && !other.isBoss && other.IsLaunched())
        {
            // 通常モンスターは n 秒後に死亡
            other.StartCoroutine(other.DelayedDie(0.5f));

            // ボスはダメージのみ受ける
            TakeDamage(1);
            return;
        }

        // 自分が通常モンスター ＆ 相手がボス ＆ 自分が Launch 状態
        if (!isBoss && other.isBoss && IsLaunched())
        {
            // 自分（通常モンスター）は n 秒後に死亡
            StartCoroutine(DelayedDie(0.5f));

            // ボスは即ダメージ
            other.TakeDamage(1);
            return;
        }

        // ===============================
        // 以下：通常モンスター同士のピンボール処理
        // ===============================
        if (!pinballEnabled) return;

        // Launch 状態かつ速度が一定以上のときのみ攻撃判定
        bool isPinballAttack =
            IsLaunched() &&
            rb.velocity.magnitude >= damageSpeedThreshold;

        if (!isPinballAttack) return;

        // 相手にダメージ
        other.TakeDamage(1);

        // ボスが含まれる場合は物理反応を行わない
        if (isBoss || other.isBoss) return;

        Rigidbody2D rb2 = other.GetComponent<Rigidbody2D>();
        if (rb2 == null) return;
    if (collision.contactCount == 0) return;

    // ✅ 중복 처리 방지(둘 다 처리하면 2번 적용돼서 이상해짐)
    if (rb.GetInstanceID() > rb2.GetInstanceID()) return;

        // 衝突面の法線ベクトル
        Vector2 n = collision.GetContact(0).normal.normalized;

        // 速度減衰
        rb.velocity *= enemyCollisionDamping;
        rb2.velocity *= enemyCollisionDamping;

        // 反発力計算
        float power = Mathf.Max(1.5f, rb.velocity.magnitude * 0.8f);
        Vector2 impulse = n * power;

        // 反発力を加える
        rb.AddForce(impulse, ForceMode2D.Impulse);
        rb2.AddForce(-impulse, ForceMode2D.Impulse);

        // 相互に Launch 状態にする
        LaunchByEnemy(impulse);
        other.LaunchByEnemy(-impulse);
    }


*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    private EnemySpawner spawner;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isBoss) return;

        if (!collision.gameObject.CompareTag("Enemy")) return;
        if (!IsLaunched()) return;
        if (collision.contactCount == 0) return;

        Enemy other = collision.gameObject.GetComponent<Enemy>();
        if (other == null || other.isBoss) return;

        Rigidbody2D rb2 = other.GetComponent<Rigidbody2D>();
        if (rb2 == null) return;

        Vector2 n = collision.GetContact(0).normal.normalized;
        float push = 0.6f;

        rb.AddForce(n * push, ForceMode2D.Impulse);
        rb2.AddForce(-n * push, ForceMode2D.Impulse);
    }

        Vector2 dirToPlayer = (playerPos - rb.position).normalized;
        rb.AddForce(dirToPlayer * moveForce, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    /* =========================
      ダメージ&エフェクト
       ========================= */
    public void TakeDamage(int damage)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        currentHP -= damage;
        PlayHitFlash();

        // ===============================
        // ボス硬直処理
        // ===============================
        if (isBoss)
        {
            bossStunEndTime = Time.time + bossStunTime;
        }

        if (PlayerCombo.Instance != null)
            PlayerCombo.Instance.AddCombo();

        if (currentHP <= 0)
            Die();
    }
            return;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            PinballKickAgainstWall(collision);
            lastBounceTime = Time.time;
            return;
        }

    void PlayHitFlash()
    {
        if (sr == null) return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitFlashCoroutine());
    }

    IEnumerator HitFlashCoroutine()
    {
        sr.color = Color.black;
        yield return new WaitForSeconds(hitFlashTime);
        sr.color = originalColor;
    }

    void Die()
    {
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.AddXP(xpReward);

        if (spawner != null)
            spawner.OnEnemyDestroyed();

        Destroy(gameObject);
    }

    public void SetSpawner(EnemySpawner s)
    {
        spawner = s;
    }

    void ApplySeparation()
    {
        if (isBoss) return;

        Collider2D[] nearEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            separationRadius,
            LayerMask.GetMask("Enemy")
        );

        Vector2 pushDir = Vector2.zero;
        int count = 0;

        foreach (Collider2D col in nearEnemies)
        {
            if (col.gameObject == gameObject)
                continue;

            Vector2 dir = (Vector2)(transform.position - col.transform.position);
            float distance = dir.magnitude;

            if (distance <= 0.01f)
                continue;

            pushDir += dir.normalized / distance;
            count++;
        }

        if (count > 0)
        {
            pushDir /= count;
            rb.AddForce(pushDir * separationForce, ForceMode2D.Force);
        }

#if UNITY_EDITOR
    // 씬에서 리다이렉트 범위를 보고 싶으면 체크
    void OnDrawGizmosSelected()
    {
        if (!chainRedirect) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, redirectRadius);
    }
#endif
}
