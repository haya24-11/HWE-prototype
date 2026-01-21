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

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            level++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.5f); // 成長曲線(希望すれば調節)

            OnLevelUp?.Invoke();
        }
    }
}
