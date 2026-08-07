using UnityEngine;

[DisallowMultipleComponent]
public sealed class DiceValue : MonoBehaviour
{
    [Min(1)]
    [SerializeField] private int value = 1;

    public int Value => value;

    public void SetValue(int newValue)
    {
        value = Mathf.Max(1, newValue);
    }
}
