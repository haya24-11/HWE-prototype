using UnityEngine;
using System;
using System.Collections.Generic;

public enum UpgradeType
{
    MoveSpeed,
    MaxHP,
    Attack
}

[Serializable]
public class UpgradeOption
{
    public UpgradeType type;
    public string title;
    public string desc;

    public int intValue;
    public float floatValue;
}

public class UpgradeManager : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;
    public UpgradeUI ui;

    [Header("Amounts")]
    public float speedUp = 2f;
    public int hpUp = 3;
    public int atkUp = 1;

    void Awake()
    {
        // PlayerStats が設定されていない場合は自動的に取得
        if (stats == null)
            stats = FindObjectOfType<PlayerStats>();
    }

    void Start()
    {
        // レベルアップ時のイベントを登録
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.OnLevelUp += HandleLevelUp;

        // UI を非表示にする
        if (ui != null)
            ui.Hide();
    }

    void OnDestroy()
    {
        // イベント登録を解除
        if (PlayerLevelSystem.Instance != null)
            PlayerLevelSystem.Instance.OnLevelUp -= HandleLevelUp;
    }

    void HandleLevelUp()
    {
        // ゲームを一時停止
        Time.timeScale = 0f;

        // 重複なしで3つの強化オプションを生成
        var options = GenerateThreeOptions();

        // UI を表示
        ui.Show(options, ApplyUpgrade);
    }

    List<UpgradeOption> GenerateThreeOptions()
    {
        // 強化タイプの候補リスト
        List<UpgradeType> pool = new List<UpgradeType>()
        {
            UpgradeType.MoveSpeed,
            UpgradeType.MaxHP,
            UpgradeType.Attack
        };

        // 現在は3種類のみだが、
        // 将来的にオプションを増やしても使えるようランダム処理を行う
        Shuffle(pool);

        List<UpgradeOption> result = new List<UpgradeOption>();
        for (int i = 0; i < 3 && i < pool.Count; i++)
            result.Add(BuildOption(pool[i]));

        return result;
    }

    UpgradeOption BuildOption(UpgradeType t)
    {
        UpgradeOption o = new UpgradeOption();
        o.type = t;

        switch (t)
        {
            case UpgradeType.MoveSpeed:
                o.title = "SPEED UP";
                o.desc = $"+{speedUp}";
                o.floatValue = speedUp;
                break;

            case UpgradeType.MaxHP:
                o.title = "HP UP";
                o.desc = $"+{hpUp}";
                o.intValue = hpUp;
                break;

            case UpgradeType.Attack:
                o.title = "ATTACK UP";
                o.desc = $"+{atkUp}";
                o.intValue = atkUp;
                break;
        }
        return o;
    }

    void ApplyUpgrade(UpgradeOption opt)
    {
        if (stats == null) return;

        // 選択された強化内容をプレイヤーに適用
        switch (opt.type)
        {
            case UpgradeType.MoveSpeed:
                stats.AddMoveSpeed(opt.floatValue);
                break;

            case UpgradeType.MaxHP:
                stats.AddMaxHP(opt.intValue);
                break;

            case UpgradeType.Attack:
                stats.AddAttack(opt.intValue);
                break;
        }

        // UI を閉じてゲームを再開
        ui.Hide();
        Time.timeScale = 1f;
    }

    void Shuffle<T>(IList<T> list)
    {
        // リストの要素をランダムに並び替える
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
