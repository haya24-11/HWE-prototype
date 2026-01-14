using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Combat")]
    public int attackPower = 1;

    [Header("Scaling (optional)")]
    public int attackPerLevel = 1;   // 레벨업 1회당 공격력 증가량
    public bool applyScalingFromLevelSystem = true;

    void Start()
    {
        // 레벨 시스템이 있으면 이벤트로 연결해서 자동으로 공격력 올리기
        if (applyScalingFromLevelSystem && PlayerLevelSystem.Instance != null)
        {
            // 시작 레벨 반영(원하면)
            ApplyAttackFromLevel(PlayerLevelSystem.Instance.level);

            PlayerLevelSystem.Instance.OnLevelUp += HandleLevelUp;
        }
    }

    void OnDestroy()
    {
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.OnLevelUp -= HandleLevelUp;
    }

    void HandleLevelUp(int newLevel)
    {
        ApplyAttackFromLevel(newLevel);
        // 또는 “레벨업 때마다 +attackPerLevel” 방식으로 하고싶으면 아래 한 줄로 대체:
        // attackPower += attackPerLevel;
    }

    void ApplyAttackFromLevel(int level)
    {
        // 기본 공격력(1) + (레벨-1)*증가량
        attackPower = 1 + (level - 1) * attackPerLevel;
    }
}
