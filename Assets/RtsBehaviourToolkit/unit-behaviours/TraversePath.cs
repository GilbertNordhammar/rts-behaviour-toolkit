using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "TraversePath", menuName = "RtsBehaviourToolkit/Behaviours/TraversePath")]
    [System.Serializable]
    public class TraversePath : UnitBehaviour
    {
        [SerializeField]
        [Min(0f)]
        int _weight;

        public override void OnUpdate(CommandGroup group)
        {
            foreach (var unit in group.Units)
            {
                var finishedMask = CommandUnit.PathStatus.NoPaths | CommandUnit.PathStatus.AllPathsTraversed;
                if ((unit.Status & finishedMask) > 0)
                    unit.Remove = true;
                if (unit.Paths.CurrentPath)
                {
                    var direction = (unit.Paths.CurrentPath.NextNode - unit.Unit.Position).normalized;
                    unit.Unit.AddMovement(_weight * direction);
                }
            }
        }
    }
}

