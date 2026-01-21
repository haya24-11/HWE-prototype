using UnityEngine;
using UnityEngine.UI;

public class PlayerXPBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        var ls = PlayerLevelSystem.Instance;
        if (ls == null) return;

        slider.minValue = 0;
        slider.maxValue = ls.xpToNext;
        slider.value = ls.currentXP;
    }
}
