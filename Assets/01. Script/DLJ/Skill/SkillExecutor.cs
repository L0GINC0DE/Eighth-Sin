using UnityEngine;


public sealed class SkillContext
{
    public EnemyFSM Target { get; }
    public int DiceValue { get; }
    public bool IsBossTarget { get; }
    public int MaxCorruption { get; }
    public BattleState BattleState { get; }
    public TurnManager TurnManager { get; }
    public int ResolvedDamage { get; private set; }
    public bool TargetKilled => Target != null && Target.Die;

    public SkillContext(
        EnemyFSM target,
        int diceValue,
        bool isBossTarget,
        int maxCorruption,
        BattleState battleState = null,
        TurnManager turnManager = null)
    {
        Target = target;
        DiceValue = diceValue;
        IsBossTarget = isBossTarget;
        MaxCorruption = maxCorruption;
        BattleState = battleState;
        TurnManager = turnManager;
    }

    public void SetResolvedDamage(int damage)
    {
        ResolvedDamage = damage;
    }
}
public class SkillExecutor : MonoBehaviour
{
    public bool Execute(SkillSo skill, SkillContext context)
    {
        if (skill == null || context == null)
            return false;

        foreach (SkillEffect effect in skill.Effects)
        {
            if (effect == null)
                continue;

            effect.Apply(context);
        }

        foreach (SkillEffect penalty in skill.Penalties)
        {
            if (penalty == null)
                continue;

            penalty.Apply(context);
        }

        return true;
    }

    public int CalculateExpectedDamage(SkillSo skill, SkillContext context)
    {
        if (skill == null || context == null)
            return 0;

        foreach (SkillEffect effect in skill.Effects)
        {
            if (effect is DealDamageEffect damageEffect)
                return damageEffect.CalculateDamage(context);
        }

        return 0;
    }
}
