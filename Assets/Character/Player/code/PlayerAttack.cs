using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private bool debugLog = true;

    [Header("Knockback")]
    public float knockbackBase = 2f;
    public float knockbackPerATK = 1.5f;
    public float maxKnockback = 8f;

    void Awake()
    {
        if (stats == null)
            stats = GetComponentInParent<PlayerStats>();
    }

    void TryHit(Collider2D col)
    {
        // ✅ 자식 콜라이더를 맞춰도 Enemy(부모)를 찾음
        Enemy enemy = col.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        int atk = (stats != null) ? stats.attackPower : 1;

        // ✅ ダメージを先に
        enemy.TakeDamage(atk);

        // ✅ 押し目買い（四百） - プレーヤー - > 敵の方向
        Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

        float kb = knockbackBase + atk * knockbackPerATK;
        kb = Mathf.Min(kb, maxKnockback);

        enemy.LaunchByPlayer(dir * kb);
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);
}
