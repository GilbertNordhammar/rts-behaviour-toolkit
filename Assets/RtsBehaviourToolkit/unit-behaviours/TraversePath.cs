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
                if (unit.Status == CommandUnit.PathStatus.NoPaths || unit.Status.HasFlag(CommandUnit.PathStatus.AllPathsTraversed))
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

