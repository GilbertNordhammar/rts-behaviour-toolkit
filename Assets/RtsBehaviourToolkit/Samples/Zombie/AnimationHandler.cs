using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RtsBehaviourToolkit;

[RequireComponent(typeof(RBTUnit)), RequireComponent(typeof(Animator))]
public class AnimationHandler : MonoBehaviour
{
    // Private
    RBTUnit _unit;
    Animator _animator;

    void UpdateAnimationState(RBTUnit.OnStateChangedEvent evnt)
    {
        switch (evnt.NewState)
        {
            case RBTUnit.ActionState.Idling:
                _animator.SetTrigger("idle");
                _animator.SetFloat("PlaybackSpeed", 1.0f);
                break;
            case RBTUnit.ActionState.Attacking:
                _animator.SetTrigger("attack");
                _animator.SetFloat("PlaybackSpeed", 1.0f);
                break;
            case RBTUnit.ActionState.Moving:
                _animator.SetTrigger("walk");
                _animator.SetFloat("PlaybackSpeed", evnt.Sender.Speed);
                break;
        }
    }

    // Unity functions
    void Start()
    {
        _unit = GetComponent<RBTUnit>();
        _unit.OnStateChanged += UpdateAnimationState;
        _animator = GetComponent<Animator>();
    }

    void OnDestroy()
    {
        _unit.OnStateChanged -= UpdateAnimationState;
    }
}
