using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public int comboCount { get; private set; } = 0;

    [Header("Combo Settings")]
    public float comboResetTime = 1.5f;

    private float lastHitTime;

    public System.Action<int> onComboChanged;

    void Update()
    {
        if (comboCount > 0 && Time.time - lastHitTime > comboResetTime)
        {
            ResetCombo();
        }
    }

    public void AddCombo()
    {
        comboCount++;
        lastHitTime = Time.time;
        onComboChanged?.Invoke(comboCount);
    }

    public void ResetCombo()
    {
        comboCount = 0;
        onComboChanged?.Invoke(comboCount);
    }
}
