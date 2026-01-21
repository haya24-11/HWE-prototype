using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject rootPanel;

    [Header("Option 1")]
    public Button btn1;
    public TMP_Text title1;
    public TMP_Text desc1;

    [Header("Option 2")]
    public Button btn2;
    public TMP_Text title2;
    public TMP_Text desc2;

    [Header("Option 3")]
    public Button btn3;
    public TMP_Text title3;
    public TMP_Text desc3;

    private Action<UpgradeOption> onPick;

    public void Show(List<UpgradeOption> opts, Action<UpgradeOption> onPickCallback)
    {
        onPick = onPickCallback;

        rootPanel.SetActive(true);

        // 既存のリスナー除去
        btn1.onClick.RemoveAllListeners();
        btn2.onClick.RemoveAllListeners();
        btn3.onClick.RemoveAllListeners();

        // 表示
        SetOption(opts[0], title1, desc1, btn1);
        SetOption(opts[1], title2, desc2, btn2);
        SetOption(opts[2], title3, desc3, btn3);
    }

    void SetOption(UpgradeOption opt, TMP_Text title, TMP_Text desc, Button btn)
    {
        title.text = opt.title;
        desc.text = opt.desc;

        btn.onClick.AddListener(() => onPick?.Invoke(opt));
    }

    public void Hide()
    {
        rootPanel.SetActive(false);
    }
}

