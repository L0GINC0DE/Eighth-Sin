using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Skill_",
    menuName = "Eighth Sin/Skill Definition")]
public class SkillSo : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string skillId;
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Classification")]
    [SerializeField] private SkillType skillType;
    [SerializeField] private SkillGroup skillGroup;
    [SerializeField] private WeaponType requiredWeapon;

    [Header("Usage")]
    [Min(0)]
    [SerializeField] private int maxUsesPerTurn;

    [Header("Corruption Cost")]
    [SerializeField] private Formula corruptionCost;
    [SerializeField] private float permanentCorruptionPercent;

    [Header("Effects")]
    [SerializeReference] private List<SkillEffect> effects = new();

    [Header("Penalties")]
    [SerializeReference] private List<SkillEffect> penalties = new();

    public string SkillId => skillId;
    public string DisplayName => displayName;
    public SkillType SkillType => skillType;
    public int MaxUsesPerTurn => maxUsesPerTurn > 0 ? maxUsesPerTurn : 1;
    public WeaponType RequiredWeapon => requiredWeapon;
    public IReadOnlyList<SkillEffect> Effects => effects;
    public IReadOnlyList<SkillEffect> Penalties => penalties;
    
}
