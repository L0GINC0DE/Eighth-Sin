using System;
using UnityEngine;

[Serializable]
public abstract class SkillEffect
{
    public abstract void Apply(SkillContext context);
}
