using UnityEngine;

public class PlayerCombo : MonoBehaviour
{
    public static ComboSystem Instance;

    void Awake()
    {
        Instance = GetComponent<ComboSystem>();
    }
}
