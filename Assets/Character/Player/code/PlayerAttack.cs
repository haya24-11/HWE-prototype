using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private bool debugLog = true;

    void Awake()
    {
        if (stats == null)
            stats = GetComponentInParent<PlayerStats>();
    }

    void TryDamage(Collider2D col)
    {
        // ✅ 자식 콜라이더를 맞춰도 Enemy(부모)를 찾음
        Enemy enemy = col.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        int dmg = (stats != null) ? stats.attackPower : 1;

        if (debugLog)
            Debug.Log($"[PlayerAttack] Hit {col.name} -> Enemy:{enemy.name}, dmg:{dmg}");

        enemy.TakeDamage(dmg);
    }

    // ✅ Trigger 방식
    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    // ✅ Collision 방식(Trigger 체크 안 했을 때도 대응)
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }
}
