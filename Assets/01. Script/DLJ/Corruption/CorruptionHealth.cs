using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CorruptionHealth : MonoBehaviour
{
    public static CorruptionHealth Instance { get; private set; }

    [Header("Corruption")]
    [Min(1)]
    [SerializeField] private int baseMaxCorruption = 100;

    [Min(1)]
    [SerializeField] private int maxCorruption = 100;

    [Min(0)]
    [SerializeField] private int currentCorruption = 100;

    [SerializeField] private Image gaugeImage;

    public int BaseMaxCorruption => baseMaxCorruption;
    public int MaxCorruption => maxCorruption;
    public int CurrentCorruption => currentCorruption;
    public bool IsDepleted => currentCorruption <= 0;

    public event Action<int, int> CorruptionChanged;
    public event Action CorruptionDepleted;

    private void Reset()
    {
        gaugeImage = GetComponent<Image>();
        currentCorruption = maxCorruption;
        UpdateGauge();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (gaugeImage == null)
            gaugeImage = GetComponent<Image>();

        ClampValues();
        UpdateGauge();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        Instance = null;
    }

    private void OnValidate()
    {
        ClampValues();

        if (gaugeImage == null)
            gaugeImage = GetComponent<Image>();

        UpdateGauge();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDepleted)
            return;

        SetCurrentCorruption(currentCorruption - damage);
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        SetCurrentCorruption(currentCorruption + amount);
    }

    public void ResetHealth()
    {
        SetCurrentCorruption(maxCorruption);
    }

    public void SetMaxCorruption(int value)
    {
        int previousMaxCorruption = maxCorruption;
        int previousCurrentCorruption = currentCorruption;

        maxCorruption = Mathf.Clamp(value, 1, baseMaxCorruption);
        currentCorruption = Mathf.Min(currentCorruption, maxCorruption);

        if (maxCorruption == previousMaxCorruption &&
            currentCorruption == previousCurrentCorruption)
        {
            return;
        }

        UpdateGauge();
        CorruptionChanged?.Invoke(currentCorruption, maxCorruption);
    }

    public void ReduceMaxCorruption(int amount)
    {
        if (amount <= 0)
            return;

        SetMaxCorruption(maxCorruption - amount);
    }

    private void SetCurrentCorruption(int value)
    {
        int previousCorruption = currentCorruption;
        currentCorruption = Mathf.Clamp(value, 0, maxCorruption);

        if (currentCorruption == previousCorruption)
            return;

        UpdateGauge();
        CorruptionChanged?.Invoke(currentCorruption, maxCorruption);

        if (currentCorruption == 0)
            CorruptionDepleted?.Invoke();
    }

    private void ClampValues()
    {
        baseMaxCorruption = Mathf.Max(1, baseMaxCorruption);
        maxCorruption = Mathf.Clamp(maxCorruption, 1, baseMaxCorruption);
        currentCorruption = Mathf.Clamp(currentCorruption, 0, maxCorruption);
    }

    private void UpdateGauge()
    {
        if (gaugeImage != null)
            gaugeImage.fillAmount = (float)currentCorruption / baseMaxCorruption;
    }
}
