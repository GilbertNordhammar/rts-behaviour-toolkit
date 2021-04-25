using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "StopIfTooClose", menuName = "RtsBehaviourToolkit/Behaviours/StopIfTooClose")]
    [System.Serializable]
    public class StopIfTooClose : UnitBehaviour
    {
        [SerializeField, Min(0)]
        float _minFollowDistance = 2;

        public override void OnUpdate(CommandGroup group)
        {
            var followGroup = group as FollowGroup;
            var attackGroup = group as AttackGroup;

            if (followGroup)
            {
                foreach (var unit in followGroup.Units)
                {
                    var sqrDist = (unit.Unit.transform.position - followGroup.Target.transform.position).sqrMagnitude;
                    if (sqrDist < _minFollowDistance * _minFollowDistance)
                    {
                        unit.Unit.AddMovement(-unit.Unit.MovementSum);
                        unit.Paths.ClearPaths();
                    }
                }
            }
            else if (attackGroup)
            {
                foreach (var unit in attackGroup.Units)
                {
                    var attackRange = unit.Unit.Attack.Range;
                    var sqrDist = (unit.Unit.transform.position - attackGroup.Target.Position).sqrMagnitude;
                    if (sqrDist < attackRange * attackRange)
                    {
                        unit.Unit.AddMovement(-unit.Unit.MovementSum);
                        unit.Paths.ClearPaths();
                    }
                }
            }
        }
    }
}

