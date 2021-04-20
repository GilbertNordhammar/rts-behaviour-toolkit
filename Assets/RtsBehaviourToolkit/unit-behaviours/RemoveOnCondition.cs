using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "RemoveOnCondition", menuName = "RtsBehaviourToolkit/Behaviours/RemoveOnCondition")]
    public class RemoveOnCondition : UnitBehaviour
    {
        public override void OnUpdate(CommandGroup group)
        {
            if (group is FollowGroup)
            {
                var followGroup = group as FollowGroup;
                followGroup.RemoveImmediately = followGroup.Target == null;
            }
            else if (group is AttackGroup)
            {
                var attackGroup = group as AttackGroup;
                if (attackGroup.Target == null)
                {
                    attackGroup.RemoveImmediately = true;
                    foreach (var unit in attackGroup.Units)
                        unit.Unit.AttackTarget = null;
                }
            }
        }
    }
}

