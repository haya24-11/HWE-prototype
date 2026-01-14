/*
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
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

    [Tooltip("충돌 시 기본으로 추가되는 밀어내기(임펄스)")]
    public float pinballBaseKick = 2.5f;

    [Tooltip("충돌 상대 속도(노멀 방향)에 비례해 추가되는 킥")]
    public float pinballKickBySpeed = 0.6f;

    [Tooltip("이 속도 이상으로 부딪힐 때만 킥 적용(너무 잔잔한 충돌은 제외)")]
    public float pinballMinRelSpeed = 0.5f;

    [Tooltip("핀볼 튕김 후 속도 상한(너무 빨라지는거 방지)")]
    public float pinballMaxSpeed = 18f;

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
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (Time.time - lastBounceTime < stopChaseTime) return;

        Vector2 playerPos = (Vector2)player.position;
        float distance = Vector2.Distance(rb.position, playerPos);

        if (distance <= stopDistance)
        {
            rb.velocity *= 0.9f;
            return;
        }

        Vector2 dir = (playerPos - rb.position).normalized;
        rb.AddForce(dir * moveForce, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    public void LaunchByPlayer(Vector2 force)
    {
        rb.velocity = force;
        launchedTimer = Time.time;
        lastBounceTime = Time.time;
    }

    public void SetSpawner(EnemySpawner s) => spawner = s;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        currentHP -= damage;

        if (sr != null)
        {
            if (currentHP <= maxHP * 0.5f) sr.color = Color.red;
            else sr.color = Color.blue;
        }

        if (currentHP <= 0) Die();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 핀볼 킥: 물리 반사(머티리얼) + 추가 임펄스
        if (pinballEnabled)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                PinballKickAgainstWall(collision);
                lastBounceTime = Time.time;
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                Enemy other = collision.gameObject.GetComponent<Enemy>();

                // ✅ 기존 데미지 룰 유지
                if (IsLaunched() && rb.velocity.magnitude >= damageSpeedThreshold)
                {
                    if (other != null)
                        other.TakeDamage(1);
                }

                PinballKickEnemyVsEnemy(collision, other);
                lastBounceTime = Time.time;
                return;
            }
        }

        // 기존 상태 갱신
        if (collision.gameObject.CompareTag("Player"))
        {
            launchedTimer = Time.time;
            lastBounceTime = Time.time;
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // pinballEnabled=false일 때도 최소한 bounce 타이밍은 유지
            lastBounceTime = Time.time;
            return;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            lastBounceTime = Time.time;
        }
    }

    // ✅ 벽에 부딪힐 때: 접촉 노멀 방향으로 "툭" 밀어내기
    void PinballKickAgainstWall(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        Vector2 n = collision.GetContact(0).normal.normalized; // 벽 -> 나 방향
        float relN = Vector2.Dot(rb.velocity, -n);             // 벽 쪽으로 박는 속도(양수면 박고있음)

        if (relN < pinballMinRelSpeed) return;

        float kick = pinballBaseKick + relN * pinballKickBySpeed;
        rb.AddForce(n * kick, ForceMode2D.Impulse);

        ClampSpeed(rb, pinballMaxSpeed);
    }

    // ✅ 적-적 충돌: 둘 다 반대로 튕기고, 추가 킥까지(핀볼 범퍼 느낌)
    // ✅ 적-적 충돌: 둘 다 x,y 방향을 반대로(-velocity)로 뒤집어서 튕기기
    // ✅ 적-적 충돌: "플레이어에게 날아간 적"이 껴있을 때만 XY 반전 적용
    // ✅ 적-적 충돌: 둘 다 튕김(반사) + 핀볼 킥
void PinballKickEnemyVsEnemy(Collision2D collision, Enemy other)
{
    if (other == null) return;

    Rigidbody2D rb2 = other.GetComponent<Rigidbody2D>();
    if (rb2 == null) return;
    if (collision.contactCount == 0) return;

    // ✅ 중복 처리 방지(둘 다 처리하면 2번 적용돼서 이상해짐)
    if (rb.GetInstanceID() > rb2.GetInstanceID()) return;

    // 충돌 노멀 (other -> this 방향)
    Vector2 n = collision.GetContact(0).normal.normalized;

    Vector2 v1 = rb.velocity;
    Vector2 v2 = rb2.velocity;

    // 상대속도 (서로 박는 정도)
    Vector2 rel = v1 - v2;
    float relToward = Vector2.Dot(rel, n); // 음수면 서로 박는 중

    // 너무 잔잔한 충돌이면 무시(원하면 값 조절)
    if (-relToward < pinballMinRelSpeed) return;

    // ✅ 1) 서로 반사(Reflect)로 튕기게
    // 반사는 "각자 상대 속도 기준"이 자연스러움
    Vector2 v1Rel = v1 - v2;
    Vector2 v2Rel = v2 - v1;

    Vector2 v1Ref = Vector2.Reflect(v1Rel, n) + v2;
    Vector2 v2Ref = Vector2.Reflect(v2Rel, -n) + v1;

    rb.velocity = v1Ref;
    rb2.velocity = v2Ref;

    // ✅ 2) 핀볼 범퍼 킥(임펄스) 추가
    // 기본 킥 + 박는 속도에 비례
    float kick = pinballBaseKick + (-relToward) * pinballKickBySpeed;

    // "플레이어가 날린 적"이면 더 강하게(원하면 숫자 조절)
    bool launchedA = IsLaunched();
    bool launchedB = other.IsLaunched();
    if (launchedA || launchedB) kick *= 1.3f;

    // 서로 반대 방향으로 팡!
    rb.AddForce(n * kick, ForceMode2D.Impulse);
    rb2.AddForce(-n * kick, ForceMode2D.Impulse);

    ClampSpeed(rb, pinballMaxSpeed);
    ClampSpeed(rb2, pinballMaxSpeed);
}



    void ClampSpeed(Rigidbody2D target, float max)
    {
        float spd = target.velocity.magnitude;
        if (spd > max)
            target.velocity = target.velocity.normalized * max;
    }

    public bool IsLaunched() => Time.time - launchedTimer < launchedTime;

    void Die()
    {
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.AddXP(xpReward);

        if (spawner != null) spawner.OnEnemyDestroyed();
        Destroy(gameObject);
    }
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

    [Header("Launch State")]
    public float launchedTime = 0.3f;
    private float launchedTimer = -10f;

    [Header("Damage Condition")]
    public float damageSpeedThreshold = 2.0f;

    [Header("Rewards")]
    public int xpReward = 3;

    // ✅ 핀볼 튕김(반사 + 킥)
    [Header("Pinball Bounce")]
    public bool pinballEnabled = true;
    public float pinballBaseKick = 2.5f;
    public float pinballKickBySpeed = 0.6f;
    public float pinballMinRelSpeed = 0.5f;
    public float pinballMaxSpeed = 18f;

    // ✅ A 방식: "튕긴 내 몬스터만" 다음 몬스터로 즉시 꺾기
    [Header("Chain Redirect (A: only this enemy)")]
    public bool chainRedirect = true;
    public float redirectRadius = 10f;

    [Range(0f, 1f)]
    [Tooltip("1=완전 즉시 방향변경, 0.5=절반만 꺾임")]
    public float redirectBlend = 1.0f;

    [Tooltip("Enemy 레이어만 체크")]
    public LayerMask enemyMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        rb.mass = 0.5f;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        rb.drag = 0f;
        rb.angularDrag = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        currentHP = maxHP;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (Time.time - lastBounceTime < stopChaseTime) return;

        Vector2 playerPos = (Vector2)player.position;
        float distance = Vector2.Distance(rb.position, playerPos);

        if (distance <= stopDistance)
        {
            rb.velocity *= 0.9f;
            return;
        }

        Vector2 dirToPlayer = (playerPos - rb.position).normalized;
        rb.AddForce(dirToPlayer * moveForce, ForceMode2D.Force);

        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    public void LaunchByPlayer(Vector2 force)
    {
        rb.velocity = force;
        launchedTimer = Time.time;
        lastBounceTime = Time.time;
    }

    public void SetSpawner(EnemySpawner s) => spawner = s;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        currentHP -= damage;

        if (sr != null)
        {
            if (currentHP <= maxHP * 0.5f) sr.color = Color.blue;
            else sr.color = Color.blue;
        }

        if (currentHP <= 0) Die();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!pinballEnabled)
        {
            if (collision.gameObject.CompareTag("Player"))
                launchedTimer = Time.time;

            if (collision.gameObject.CompareTag("Player") ||
                collision.gameObject.CompareTag("Enemy") ||
                collision.gameObject.CompareTag("Wall"))
            {
                lastBounceTime = Time.time;
            }
            return;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            PinballKickAgainstWall(collision);
            lastBounceTime = Time.time;
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy other = collision.gameObject.GetComponent<Enemy>();

            // ✅ 데미지 규칙 유지
            if (IsLaunched() && rb.velocity.magnitude >= damageSpeedThreshold)
            {
                if (other != null)
                    other.TakeDamage(1);
            }

            PinballKickEnemyVsEnemy(collision, other);
            lastBounceTime = Time.time;
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            launchedTimer = Time.time;
            lastBounceTime = Time.time;
        }
    }

    void PinballKickAgainstWall(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        Vector2 n = collision.GetContact(0).normal.normalized; // 벽 -> 나 방향
        float relN = Vector2.Dot(rb.velocity, -n);             // 벽 쪽으로 박는 속도
        if (relN < pinballMinRelSpeed) return;

        float kick = pinballBaseKick + relN * pinballKickBySpeed;
        rb.AddForce(n * kick, ForceMode2D.Impulse);

        ClampSpeed(rb, pinballMaxSpeed);
    }

    // ✅ 적-적 충돌: 반사 + 킥, 그리고 A 방식 리다이렉트(내 몬스터만)
    void PinballKickEnemyVsEnemy(Collision2D collision, Enemy other)
    {
        if (other == null) return;

        Rigidbody2D rb2 = other.GetComponent<Rigidbody2D>();
        if (rb2 == null) return;
        if (collision.contactCount == 0) return;

        // ✅ 중복 처리 방지(한쪽에서만 계산)
        bool iAmSolver = rb.GetInstanceID() < rb2.GetInstanceID();
        if (!iAmSolver) return;

        Vector2 n = collision.GetContact(0).normal.normalized; // other -> this

        Vector2 v1 = rb.velocity;
        Vector2 v2 = rb2.velocity;

        Vector2 rel = v1 - v2;
        float relToward = Vector2.Dot(rel, n); // 음수면 서로 박는 중
        if (-relToward < pinballMinRelSpeed) return;

        // 1) 반사(상대속도 기준)
        Vector2 v1Rel = v1 - v2;
        Vector2 v2Rel = v2 - v1;

        Vector2 v1Ref = Vector2.Reflect(v1Rel, n) + v2;
        Vector2 v2Ref = Vector2.Reflect(v2Rel, -n) + v1;

        rb.velocity = v1Ref;
        rb2.velocity = v2Ref;

        // 2) 킥(임펄스)
        float kick = pinballBaseKick + (-relToward) * pinballKickBySpeed;
        if (IsLaunched() || other.IsLaunched()) kick *= 1.3f;

        rb.AddForce(n * kick, ForceMode2D.Impulse);
        rb2.AddForce(-n * kick, ForceMode2D.Impulse);

        ClampSpeed(rb, pinballMaxSpeed);
        ClampSpeed(rb2, pinballMaxSpeed);

        // ✅ A: 내 몬스터(= 이 스크립트가 붙은 객체)만 "다음 몬스터"로 즉시 꺾기
        // (solver가 나일 때만 실행되니까 중복 적용 없음)
        RedirectThisVelocityToNextEnemy(other);
    }

    // ✅ 다음 몬스터 방향으로 '내 속도 방향'만 즉시 변경
    void RedirectThisVelocityToNextEnemy(Enemy excludeOther)
    {
        if (!chainRedirect) return;

        float speed = rb.velocity.magnitude;
        if (speed <= 0.01f) return;

        // 가까운 Enemy 탐색 (자기 자신 + 방금 부딪힌 other 제외)
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, redirectRadius, enemyMask);

        Transform best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponentInParent<Enemy>();
            if (e == null) continue;
            if (e == this) continue;
            if (excludeOther != null && e == excludeOther) continue;

            float d = ((Vector2)e.transform.position - rb.position).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                best = e.transform;
            }
        }

        if (best == null) return;

        Vector2 dir = ((Vector2)best.position - rb.position).normalized;

        // 완전 강제(1) 또는 섞기(0~1)
        Vector2 newDir = Vector2.Lerp(rb.velocity.normalized, dir, redirectBlend).normalized;

        rb.velocity = newDir * speed; // ✅ 속도 유지, 방향만 변경
    }

    void ClampSpeed(Rigidbody2D target, float max)
    {
        float spd = target.velocity.magnitude;
        if (spd > max)
            target.velocity = target.velocity.normalized * max;
    }

    public bool IsLaunched() => Time.time - launchedTimer < launchedTime;

    void Die()
    {
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.AddXP(xpReward);

        if (spawner != null) spawner.OnEnemyDestroyed();
        Destroy(gameObject);
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
