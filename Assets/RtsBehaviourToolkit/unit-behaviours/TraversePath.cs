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
            var isGoTo = group is GoToGroup;
            foreach (var unit in group.Units)
            {
                if (isGoTo)
                    unit.Remove = !unit.Paths.CurrentPath;

                if (unit.Paths.CurrentPath && !unit.Paths.CurrentPath.Traversed && !unit.Remove)
                {
                    var direction = (unit.Paths.CurrentPath.NextNode - unit.Unit.Position).normalized;
                    unit.Unit.AddMovement(_weight * direction);
                }
            }
        }
    }
}

