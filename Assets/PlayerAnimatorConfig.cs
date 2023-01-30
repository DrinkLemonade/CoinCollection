using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerAnimatorConfig : ScriptableObject
{
    [SerializeField]
    float walkAnimationSpeed, idleAnimationSpeed = 1f;

    [SerializeField]
    AnimationClip walk, idle = default;

    public AnimationClip Walk => walk;
    public float WalkAnimationSpeed => walkAnimationSpeed;

    public AnimationClip Idle => idle;
    public float IdleAnimationSpeed => idleAnimationSpeed;
}