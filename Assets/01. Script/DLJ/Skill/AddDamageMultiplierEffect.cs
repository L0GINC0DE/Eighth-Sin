using System;
using UnityEngine;

[Serializable]
public sealed class AddDamageMultiplierEffect : SkillEffect
{
    [Min(0f)]
    public float multiplier = 1f;

    public override void Apply(SkillContext context)
    {
        context?.BattleState?.AddDamageMultiplier(multiplier);
    }
}
