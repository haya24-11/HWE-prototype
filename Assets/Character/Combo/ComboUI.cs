using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    public TextMeshProUGUI comboText;
    public ComboSystem comboSystem;

    void Start()
    {
        comboText.gameObject.SetActive(false);
        comboSystem.onComboChanged += UpdateCombo;
    }

    void UpdateCombo(int combo)
    {
        if (combo <= 1)
        {
            comboText.gameObject.SetActive(false);
        }
        else
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"Combo x{combo}";
        }
    }
}
