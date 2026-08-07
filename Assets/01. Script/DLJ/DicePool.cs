using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public sealed class DicePool : MonoBehaviour
{
    [Min(1)]
    [SerializeField] private int dicePerTurn = 3;
    [Min(2)]
    [SerializeField] private int fallbackDiceSides = 6;
    [SerializeField] private DiceRotation diceRotation;

    private readonly List<int> lastRoll = new();
    private TurnManager turnManager;

    public IReadOnlyList<int> DiceValues => turnManager.State.DiceValues;
    public IReadOnlyList<int> LastRoll => lastRoll;
    public int DicePerTurn => dicePerTurn;

    public event Action<IReadOnlyList<int>> OnDiceRolled;
    public event Action<IReadOnlyList<int>> OnPoolChanged;

    public void Initialize(TurnManager manager)
    {
        turnManager = manager;

        if (diceRotation == null)
            diceRotation = FindFirstObjectByType<DiceRotation>();
    }

    public void RollForPlayerTurn()
    {
        if (turnManager == null ||
            turnManager.State.IsBattleOver ||
            turnManager.CurrentTurn != Team.Player)
        {
            return;
        }

        lastRoll.Clear();
        turnManager.State.ClearDice();

        if (diceRotation != null)
        {
            IReadOnlyList<int> rolledValues = diceRotation.RollDice(dicePerTurn);
            lastRoll.AddRange(rolledValues);
        }
        else
        {
            for (int i = 0; i < dicePerTurn; i++)
                lastRoll.Add(Random.Range(1, fallbackDiceSides + 1));
        }

        foreach (int value in lastRoll)
            turnManager.State.AddDie(value);

        OnDiceRolled?.Invoke(lastRoll);
        NotifyPoolChanged();
    }

    public bool TryConsumeDieValue(int value)
    {
        if (turnManager == null || turnManager.CurrentTurn != Team.Player)
            return false;

        bool consumed = turnManager.State.TryConsumeDieValue(value);

        if (consumed)
            NotifyPoolChanged();

        return consumed;
    }

    public bool TryEndPlayerTurnIfEmpty()
    {
        if (turnManager == null)
            return false;

        return turnManager.TryEndPlayerTurnIfDiceDepleted();
    }

    private void NotifyPoolChanged()
    {
        OnPoolChanged?.Invoke(turnManager.State.DiceValues);
    }
}
