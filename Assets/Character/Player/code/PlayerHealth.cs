using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 10;
    public int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHP = Mathf.Max(0, currentHP - dmg);
        if (currentHP == 0)
        {
            // 사망 처리 원하면 여기서
            // Debug.Log("Player Dead");
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
}
