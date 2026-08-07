using UnityEngine;

[DisallowMultipleComponent]
public sealed class SkillPresentationSettings : MonoBehaviour
{
    [Header("Dice Insertion")]
    [Min(0f)]
    [SerializeField] private float insertDistance = 0.5f;
    [Min(0f)]
    [SerializeField] private float insertDepth = 0.3f;
    [Min(0.01f)]
    [SerializeField] private float insertDuration = 0.7f;
    [Range(0.1f, 1f)]
    [SerializeField] private float insertedScale = 0.9f;
    [Range(0.05f, 1f)]
    [SerializeField] private float scaleDurationRatio = 0.4f;

    [Header("Skill Exit")]
    [Min(0f)]
    [SerializeField] private float insertedHoldDuration = 0.2f;
    [Min(0f)]
    [SerializeField] private float exitDistance = 5f;
    [Min(0.01f)]
    [SerializeField] private float exitDuration = 0.7f;

    public float InsertDistance => insertDistance;
    public float InsertDepth => insertDepth;
    public float InsertDuration => insertDuration;
    public float InsertedScale => insertedScale;
    public float ScaleDurationRatio => scaleDurationRatio;
    public float InsertedHoldDuration => insertedHoldDuration;
    public float ExitDistance => exitDistance;
    public float ExitDuration => exitDuration;
}
