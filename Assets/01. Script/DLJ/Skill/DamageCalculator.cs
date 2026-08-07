using UnityEngine;

public static class DamageCalculator
{
    public static int Calculate(int baseDamage, BattleState battleState, int defense)
    {
        int flatBonus = battleState?.FlatDamageBonus ?? 0;
        float multiplier = battleState?.DamageMultiplier ?? 1f;
        return Calculate(baseDamage, flatBonus, multiplier, defense);
    }

    public static int Calculate(
        int baseDamage,
        int flatDamageBonus,
        float damageMultiplier,
        int defense)
    {
        int damageBeforeMultiplier = Mathf.Max(0, baseDamage + flatDamageBonus);
        float safeMultiplier = Mathf.Max(0f, damageMultiplier);
        float defenseMultiplier = 100f / (100f + Mathf.Max(0, defense));
        float damage = damageBeforeMultiplier * safeMultiplier * defenseMultiplier;
        return Mathf.CeilToInt(damage);
    }
}
