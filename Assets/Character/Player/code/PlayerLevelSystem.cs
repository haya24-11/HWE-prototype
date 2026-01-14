using UnityEngine;
using System;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }

    public int level = 1;
    public int currentXP = 0;
    public int xpToNext = 10;

    public int baseXpToNext = 10;
    public int xpIncreasePerLevel = 5;

    public event Action<int> OnLevelUp; // ✅ 추가

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Recalc();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            Recalc();

            OnLevelUp?.Invoke(level); // ✅ 추가
        }
    }

    void Recalc()
    {
        xpToNext = baseXpToNext + (level - 1) * xpIncreasePerLevel;
    }
}
