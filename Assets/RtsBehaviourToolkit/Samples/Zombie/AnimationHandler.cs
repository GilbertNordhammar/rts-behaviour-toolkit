using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RtsBehaviourToolkit;

[RequireComponent(typeof(RBTUnit)), RequireComponent(typeof(Animator))]
public class AnimationHandler : MonoBehaviour
{
    // Inspector and public
    [SerializeField]
    string _attackClipName;

    // Private
    RBTUnit _unit;
    Animator _animator;
    AnimationClip _attackClip;
    IEnumerator _attackLoop;

    void UpdateAnimationState(RBTUnit.OnStateChangedEvent evnt)
    {
        if (_attackLoop != null)
            StopCoroutine(_attackLoop);

        var state = evnt.NewState;
        if (state.HasFlag(RBTUnit.UnitState.Dead))
        {
            _animator.SetTrigger(AnimTrigger.Die);
        }
        else if (state.HasFlag(RBTUnit.UnitState.Attacking))
        {
            _attackLoop = AttackLoop(evnt.Sender.Attack);
            StartCoroutine(_attackLoop);
        }
        else if (state.HasFlag(RBTUnit.UnitState.Moving))
        {
            _animator.SetTrigger(AnimTrigger.Walk);
            _animator.SetFloat(AnimVar.WalkMultiplier, evnt.Sender.Speed);
        }
        else if (state.HasFlag(RBTUnit.UnitState.Idling))
        {
            _animator.SetTrigger(AnimTrigger.Idle);
            _animator.SetFloat(AnimVar.IdleMultiplier, 1.0f);
        }

    }

    IEnumerator AttackLoop(AttackInfo attack)
    {
        var playbackMult = attack.TimePerAttack < _attackClip.length
            ? _attackClip.length / attack.TimePerAttack
            : 1;

        _animator.SetFloat(AnimVar.AttackMultiplier, playbackMult);

        while (true)
        {
            yield return new WaitForSeconds(attack.TimePerAttack);
            _animator.SetTrigger(AnimTrigger.Attack);
        }
    }

    static class AnimTrigger
    {
        public static string Idle { get => "idle"; }
        public static string Walk { get => "walk"; }
        public static string Attack { get => "attack"; }
        public static string Die { get => "die"; }
    }

    static class AnimVar
    {
        public static string IdleMultiplier { get => "idleMultiplier"; }
        public static string WalkMultiplier { get => "walkMultiplier"; }
        public static string AttackMultiplier { get => "attackMultiplier"; }
    }

    // Unity functions
    void Start()
    {
        _unit = GetComponent<RBTUnit>();
        _unit.OnStateChanged += UpdateAnimationState;
        _animator = GetComponent<Animator>();

        var controller = _animator.runtimeAnimatorController;
        foreach (var clip in controller.animationClips)
        {
            if (clip.name == _attackClipName)
                _attackClip = clip;
        }

        if (!_attackClip)
            Debug.Log($"Couldn't find any attack clip '{_attackClipName}' on animation controller attached to gameobejct '{name}'");
    }

    void OnDestroy()
    {
        _unit.OnStateChanged -= UpdateAnimationState;
    }
}
