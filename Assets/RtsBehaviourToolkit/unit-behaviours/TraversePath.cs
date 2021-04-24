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
                UpdatePaths(unit);

                if (isGoTo)
                    unit.Remove = unit.Remove || !unit.Paths.CurrentPath;

                if (unit.Paths.CurrentPath && !unit.Paths.CurrentPath.Traversed && !unit.Remove)
                {
                    var direction = (unit.Paths.CurrentPath.NextNode - unit.Unit.Position).normalized;
                    unit.Unit.AddMovement(_weight * direction);
                }
            }
        }

        void UpdatePaths(CommandUnit unit)
        {
            if (unit.Paths.Count > 0 && unit.Paths.CurrentPath.Traversed)
                unit.Paths.PopPath();
            if (unit.Paths.Count == 0) return;

            var sqrOffset = unit.Paths.CurrentPath.NextNode - unit.Unit.Position;
            sqrOffset = Vector3.Scale(sqrOffset, sqrOffset);
            var samePosXZ = sqrOffset.x < 0.01f && sqrOffset.z < 0.01f;
            var sameAltitude = sqrOffset.y < 1.0; // TODO: Exchange 1.0 with unit height variable
            var sqrStepSize = Mathf.Pow(unit.Unit.Speed * Time.fixedDeltaTime, 2); // Is this the actual step size?
            var sqrDistXZ = new Vector3(sqrOffset.x, 0, sqrOffset.z).sqrMagnitude;
            var reachedNextNode = samePosXZ && sameAltitude || sqrDistXZ < sqrStepSize;

            if (reachedNextNode)
                unit.Paths.CurrentPath.Increment();
        }
    }
}

