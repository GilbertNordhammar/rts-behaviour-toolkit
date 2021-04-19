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

            if (followGroup)
            {
                foreach (var unit in followGroup.Units)
                {
                    var sqrDist = (unit.Unit.transform.position - followGroup.Target.transform.position).sqrMagnitude;
                    if (sqrDist < _minFollowDistance * _minFollowDistance)
                        unit.Paths.ClearPaths();
                }
            }
        }
    }
}

