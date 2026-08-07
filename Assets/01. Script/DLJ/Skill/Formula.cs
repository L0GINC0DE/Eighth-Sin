using System;
using UnityEngine;

public enum ValueSource
{
    Fixed,
    Dice,
    TargetMaxHealth,
    MaxCorruption
}

[Serializable]
public struct Formula
{
    public ValueSource source;
    public float multiplier;
    public int flatBonus;

    public int Evaluate(SkillContext context)
    {
        float value = source switch
        {
            ValueSource.Fixed => 0,
            ValueSource.Dice => context.DiceValue,
            ValueSource.TargetMaxHealth => context.Target != null ? context.Target.HP : 0,
            ValueSource.MaxCorruption => context.MaxCorruption,
            _ => 0
        };

        return Mathf.CeilToInt(value * multiplier + flatBonus);
    }
}
