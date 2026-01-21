using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public UpgradeType type;

    [Header("UI")]
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Level")]
    public int maxLevel = 5;

    [Header("Value Per Level")]
    public float valuePerLevel = 1f; // 攻撃力は+1、移動速度は+0.5といった具合
}
