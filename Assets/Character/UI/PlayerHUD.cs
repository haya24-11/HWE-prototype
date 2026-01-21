/*
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Auto Find (비워두면 자동으로 찾음)")]
    public PlayerHealth health;
    public PlayerStats stats;
    public PlayerLevelSystem levelSystem;

    [Header("UI")]
    public TMP_Text hpText;
    public TMP_Text atkText;
    public TMP_Text xpText;
    public Slider xpSlider; // 없어도 됨(비워도 OK)

    void Awake()
    {
        // 자동 찾기: 플레이어 태그 사용 권장
        if (health == null || stats == null || levelSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (health == null) health = player.GetComponent<PlayerHealth>();
                if (stats == null) stats = player.GetComponent<PlayerStats>();
                if (levelSystem == null) levelSystem = player.GetComponent<PlayerLevelSystem>();
            }

            // 혹시 LevelSystem이 플레이어가 아니라 다른 오브젝트에 있으면 싱글톤으로도 잡기
            if (levelSystem == null) levelSystem = PlayerLevelSystem.Instance;
        }
    }

    void Update()
    {
        // HP
        if (hpText != null && health != null)
            hpText.text = $"HP  {health.currentHP} / {health.maxHP}";
        else if (hpText != null)
            hpText.text = "HP  -";

        // ATK
        if (atkText != null && stats != null)
            atkText.text = $"ATK  {stats.attackPower}";
        else if (atkText != null)
            atkText.text = "ATK  -";

        // XP
        if (xpText != null && levelSystem != null)
            xpText.text = $"XP  {levelSystem.currentXP} / {levelSystem.xpToNext}   (Lv {levelSystem.level})";
        else if (xpText != null)
            xpText.text = "XP  -";

        // XP Bar
        if (xpSlider != null && levelSystem != null)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = levelSystem.xpToNext;
            xpSlider.value = levelSystem.currentXP;
        }
    }
}
*/

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;
    public PlayerLevelSystem level;

    [Header("UI")]
    public TMP_Text hpText;
    public TMP_Text atkText;
    public TMP_Text xpText;
    public TMP_Text speedText;
    public Slider xpSlider;

    void Awake()
    {
        if (stats == null) stats = FindObjectOfType<PlayerStats>();
        if (level == null) level = FindObjectOfType<PlayerLevelSystem>();
    }

    void Update()
    {
        if (stats != null)
        {
            if (hpText != null) hpText.text = $"HP {stats.currentHP}/{stats.maxHP}";
            if (atkText != null) atkText.text = $"ATK {stats.attackPower}";
            if (speedText != null) speedText.text = $"SPEED {stats.moveSpeed}";
        }

        if (level != null)
        {
            if (xpText != null) xpText.text = $"XP {level.currentXP}/{level.xpToNext} (LV {level.level})";

            if (xpSlider != null)
            {
                xpSlider.maxValue = level.xpToNext;
                xpSlider.value = level.currentXP;
            }
        }
    }
}
