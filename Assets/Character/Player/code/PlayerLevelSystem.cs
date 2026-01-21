using UnityEngine;
using System;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }

    [Header("Level")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNext = 10;

    public event Action OnLevelUp;

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
            xpToNext = Mathf.RoundToInt(xpToNext * 1.5f); // 成長曲線(希望すれば調節)

            OnLevelUp?.Invoke();
        }

    void Recalc()
    {
        xpToNext = baseXpToNext + (level - 1) * xpIncreasePerLevel;
    }
}
