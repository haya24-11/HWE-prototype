using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 10;
    public int currentHP;

    [Header("Stats")]
    public int attackPower = 1;
    public float moveSpeed = 5f;

    [Header("Damage")]
    public float invincibleTime = 0.5f;
    private float lastDamagedTime = -999f;

    void Awake()
    {
        currentHP = maxHP;
    }

    public bool CanTakeDamage()
    {
        return Time.time - lastDamagedTime >= invincibleTime;
    }

    public void TakeDamage(int dmg)
    {
        if (!CanTakeDamage()) return;
        if (dmg <= 0) return;

        lastDamagedTime = Time.time;
        currentHP = Mathf.Max(0, currentHP - dmg);

        Debug.Log($"Player HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    void Die()
    {
        Debug.Log("Player Dead");
        // ゲームオーバー処理
    }

    /* ===== レベルアップ補償 ===== */
    public void AddMaxHP(int amount)
    {
        maxHP += amount;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public void AddAttack(int amount)
    {
        attackPower += amount;
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }
}
