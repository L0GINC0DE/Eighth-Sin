using System;

[Serializable]
public sealed class DealDamageEffect : SkillEffect
{
    public Formula baseDamage;

    public override void Apply(SkillContext context)
    {
        if (context?.Target == null)
            return;

        EnemyCombatStats stats = context.Target.GetComponent<EnemyCombatStats>();
        int finalDamage = CalculateDamage(context);

        if (stats != null)
            stats.ApplyDamage(finalDamage);
        else
        {
            context.Target.HPDown(finalDamage);

            if (context.Target.NowHP <= 0)
                context.Target.Die = true;
        }

        context.SetResolvedDamage(finalDamage);
        context.BattleState?.ClearTemporaryDamageModifiers();
    }

    public int CalculateDamage(SkillContext context)
    {
        if (context?.Target == null)
            return 0;

        EnemyCombatStats stats = context.Target.GetComponent<EnemyCombatStats>();
        int defense = stats != null ? stats.Defense : 0;
        int rawDamage = baseDamage.Evaluate(context);
        return DamageCalculator.Calculate(rawDamage, context.BattleState, defense);
    }
}
