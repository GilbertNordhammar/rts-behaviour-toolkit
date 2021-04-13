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
                var foundNearbyPos = false;
                var preNextIndex = evnt.unit.CurrentPath.NextNodeIndex;
                while (!evnt.unit.CurrentPath.Traversed && !foundNearbyPos)
                {
                    NavMeshHit navMeshHit;
                    foundNearbyPos = NavMesh.SamplePosition(evnt.unit.CurrentPath.NextNode, out navMeshHit, 1f, navMeshMask); // is 1f good enough??
                    if (foundNearbyPos)
                        evnt.unit.CurrentPath.NextNode = navMeshHit.position;
                    else
                        evnt.unit.CurrentPath.Increment();
                }

                if (evnt.unit.CurrentPath.Traversed)
                    return;

                var invalidNextNode = preNextIndex != evnt.unit.CurrentPath.NextNodeIndex;

                var blockedPath = false;
                if (!invalidNextNode)
                {
                    var mask = RBTConfig.WalkableMask | RBTConfig.UnitMask;
                    mask = ~mask;
                    var posOffset = evnt.unit.CurrentPath.NextNode - evnt.unit.Unit.Position;

                    blockedPath = Physics.Raycast(evnt.unit.Unit.Position, posOffset.normalized, posOffset.magnitude, mask);
                }

                if (blockedPath || invalidNextNode)
                {
                    var path = new NavMeshPath();
                    NavMesh.CalculatePath(evnt.unit.Unit.Position, evnt.unit.CurrentPath.NextNode, NavMesh.AllAreas, path);
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        evnt.unit.CurrentPath.Increment();
                        evnt.unit.PushPath(path.corners);
                    }
                }
            };
        }

        public override void OnUpdate(CommandGroup group)
        {
            foreach (var unit in group.Units)
            {
                var direction = (unit.CurrentPath.NextNode - unit.Unit.Position).normalized;
                unit.Unit.AddMovement(_weight * direction);
            }
        }
    }
}

