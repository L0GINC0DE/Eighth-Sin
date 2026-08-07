using UnityEngine;

public class ConditionalEffect
{
    public SkillTrigger trigger;
    [SerializeReference] public SkillEffect effect;
}

public enum SkillTrigger
{
    OnCast,
    OnDamageResolved,
    OnTargetKilled,
    OnTargetSurvived,
    OnBattleEnd,
    OnStageEnd
}
