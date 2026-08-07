using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BattleState
{
    [Header("Turn")]
    [SerializeField] private Team currentTurn = Team.Player;

    [Header("Dice")]
    [SerializeField] private List<int> diceValues = new();

    [Header("Corruption")]
    [Min(1)]
    [SerializeField] private int maxCorruption = 100;
    [Min(0)]
    [SerializeField] private int corruption;
    [Min(0)]
    [SerializeField] private int permanentCorruption;

    [Header("Temporary Damage Modifiers")]
    [SerializeField] private int flatDamageBonus;
    [SerializeField] private float damageMultiplierSum;
    [SerializeField] private int multiplierStackCount;

    [Header("Battle Result")]
    [SerializeField] private bool isBattleOver;
    [SerializeField] private bool playerWon;

    public Team CurrentTurn => currentTurn;
    public IReadOnlyList<int> DiceValues => diceValues;
    public bool HasDice => diceValues.Count > 0;

    public int MaxCorruption => maxCorruption;
    public int Corruption => corruption;
    public int PermanentCorruption => permanentCorruption;
    public bool IsCorruptionLethal => corruption >= maxCorruption;

    public int FlatDamageBonus => flatDamageBonus;
    public float DamageMultiplier => multiplierStackCount == 0
        ? 1f
        : damageMultiplierSum;

    public bool IsBattleOver => isBattleOver;
    public bool PlayerWon => playerWon;

    public void BeginBattle(Team startingTurn = Team.Player)
    {
        diceValues ??= new List<int>();
        currentTurn = startingTurn;
        isBattleOver = false;
        playerWon = false;
        ClearDice();
        ClearTemporaryDamageModifiers();
        ClampCorruptionValues();
    }

    internal void SetTurn(Team team)
    {
        currentTurn = team;
    }

    public bool AddDie(int value)
    {
        if (value < 1)
            return false;

        diceValues.Add(value);
        return true;
    }

    public bool TryConsumeDieAt(int index, out int value)
    {
        if (index < 0 || index >= diceValues.Count)
        {
            value = 0;
            return false;
        }

        value = diceValues[index];
        diceValues.RemoveAt(index);
        return true;
    }

    public bool TryConsumeDieValue(int value)
    {
        int index = diceValues.IndexOf(value);

        if (index < 0)
            return false;

        diceValues.RemoveAt(index);
        return true;
    }

    public void ClearDice()
    {
        diceValues.Clear();
    }

    public void AddCorruption(int amount)
    {
        if (amount <= 0)
            return;

        corruption = Mathf.Min(maxCorruption, corruption + amount);
    }

    public void SetPermanentCorruption(int amount)
    {
        permanentCorruption = Mathf.Clamp(amount, 0, maxCorruption);
        corruption = Mathf.Max(corruption, permanentCorruption);
    }

    public int ApplyVictoryRecovery()
    {
        int recoverableCorruption = Mathf.Max(0, corruption - permanentCorruption);
        int recovery = Mathf.CeilToInt(recoverableCorruption * 0.3f);
        corruption = Mathf.Max(permanentCorruption, corruption - recovery);
        return recovery;
    }

    public void AddFlatDamageBonus(int amount)
    {
        flatDamageBonus += amount;
    }

    public void AddDamageMultiplier(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        damageMultiplierSum += multiplier;
        multiplierStackCount++;
    }

    public void ClearTemporaryDamageModifiers()
    {
        flatDamageBonus = 0;
        damageMultiplierSum = 0f;
        multiplierStackCount = 0;
    }

    public void EndBattle(bool didPlayerWin)
    {
        isBattleOver = true;
        playerWon = didPlayerWin;
        ClearDice();
        ClearTemporaryDamageModifiers();
    }

    private void ClampCorruptionValues()
    {
        maxCorruption = Mathf.Max(1, maxCorruption);
        permanentCorruption = Mathf.Clamp(permanentCorruption, 0, maxCorruption);
        corruption = Mathf.Clamp(corruption, permanentCorruption, maxCorruption);
    }
}
