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

        public override void OnCommandGroupCreated(CommandGroup group)
        {
            group.OnNewPathNode += (evnt) =>
            {
                // TODO: Clean this up
                var navMeshMask = 1 << NavMesh.GetAreaFromName("Walkable");
                var cornerIndex = evnt.unit.NextCornerIndex;
                var pathLength = evnt.unit.CurrentPath.Nodes.Length;

                NavMeshHit navMeshHit;
                while (cornerIndex < pathLength && !NavMesh.SamplePosition(evnt.unit.CurrentPath.Nodes[cornerIndex], out navMeshHit, 0.01f, navMeshMask))
                {
                    cornerIndex++;
                }
                if (cornerIndex < pathLength)
                {
                    var nextCornerIndex = evnt.unit.CurrentPath.NextCornerIndex;
                    evnt.unit.CurrentPath.Nodes[nextCornerIndex] = evnt.unit.CurrentPath.Nodes[cornerIndex];
                }

                var mask = RBTConfig.WalkableMask | RBTConfig.UnitMask;
                mask = ~mask;
                var posOffset = evnt.unit.CurrentPath.NextCorner - evnt.unit.CurrentPath.PreviousCorner;
                RaycastHit hit;
                var blockedPath = Physics.Raycast(evnt.unit.CurrentPath.PreviousCorner, posOffset.normalized, out hit, posOffset.magnitude, mask);
                if (blockedPath)
                {
                    var path = new NavMeshPath();
                    NavMesh.CalculatePath(evnt.unit.CurrentPath.PreviousCorner, evnt.unit.CurrentPath.NextCorner, NavMesh.AllAreas, path);
                    if (path.status == NavMeshPathStatus.PathComplete)
                        evnt.unit.PushPath(path.corners);
                }
            };
        }

        public override void OnUpdate(CommandGroup group)
        {
            foreach (var unit in group.Units)
            {
                // var direction = unit.OffsetToNextCorner.normalized;
                var direction = (unit.CurrentPath.NextCorner - unit.Unit.transform.position).normalized;

                unit.Unit.AddMovement(_weight * direction);
            }
        }
    }
}

