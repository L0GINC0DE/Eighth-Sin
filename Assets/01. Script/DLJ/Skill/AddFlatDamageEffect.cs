using System;

[Serializable]
public sealed class AddFlatDamageEffect : SkillEffect
{
    public Formula amount;

    public override void Apply(SkillContext context)
    {
        if (context?.BattleState == null)
            return;

        context.BattleState.AddFlatDamageBonus(amount.Evaluate(context));
    }
}
