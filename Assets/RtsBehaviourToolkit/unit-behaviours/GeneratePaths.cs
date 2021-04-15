using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "GeneratePaths", menuName = "RtsBehaviourToolkit/Behaviours/GeneratePaths")]
    [System.Serializable]
    public class GeneratePaths : UnitBehaviour
    {
        // Public
        public override void OnCommandGroupCreated(CommandGroup commandGroup)
        {
            if (commandGroup is GoToGroup)
            {
                var goToGroup = (GoToGroup)commandGroup;
                AddHighLevelPaths(goToGroup, goToGroup.Destination);

                commandGroup.OnNewPathNode += (evnt) => EnsureNextNodeIsReachable(evnt.unit);
            }
            else if (commandGroup is PatrolGroup)
            {
                throw new NotImplementedException();
            }
            else if (commandGroup is FollowGroup)
            {
                throw new NotImplementedException();
            }
            else // attack group
            {
                throw new NotImplementedException();
            }
        }

        // Private
        void AddHighLevelPaths(CommandGroup group, Vector3 destination)
        {
            var units = group.Units;
            var center = new Vector3();
            foreach (var unit in units)
            {
                center += unit.Unit.Position;
            }
            center /= units.Count;

            var navMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(center, destination, NavMesh.AllAreas, navMeshPath);

            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                foreach (var unit in units)
                {
                    var posOffset = unit.Unit.Position - center;
                    var nodes = new Vector3[navMeshPath.corners.Length];
                    Array.Copy(navMeshPath.corners, nodes, navMeshPath.corners.Length);
                    for (int j = 0; j < nodes.Length; j++)
                        nodes[j] += posOffset;
                    unit.Paths.PushPath(nodes);
                }
            }
        }

        void EnsureNextNodeIsReachable(CommandUnit unit)
        {
            // TODO: Clean this up
            var navMeshMask = 1 << NavMesh.GetAreaFromName("Walkable");
            var foundNearbyPos = false;
            var preNextIndex = unit.Paths.CurrentPath.NextNodeIndex;
            while (!unit.Paths.CurrentPath.Traversed && !foundNearbyPos)
            {
                NavMeshHit navMeshHit;
                foundNearbyPos = NavMesh.SamplePosition(unit.Paths.CurrentPath.NextNode, out navMeshHit, 1f, navMeshMask); // is 1f good enough??
                if (foundNearbyPos)
                    unit.Paths.CurrentPath.NextNode = navMeshHit.position;
                else
                    unit.Paths.CurrentPath.Increment();
            }

            if (unit.Paths.CurrentPath.Traversed)
                return;

            var invalidNextNode = preNextIndex != unit.Paths.CurrentPath.NextNodeIndex;

            var blockedPath = false;
            if (!invalidNextNode)
            {
                var mask = RBTConfig.WalkableMask | RBTConfig.UnitMask;
                mask = ~mask;
                var posOffset = unit.Paths.CurrentPath.NextNode - unit.Unit.Position;

                blockedPath = Physics.Raycast(unit.Unit.Position, posOffset.normalized, posOffset.magnitude, mask);
            }

            if (blockedPath || invalidNextNode)
            {
                var path = new NavMeshPath();
                NavMesh.CalculatePath(unit.Unit.Position, unit.Paths.CurrentPath.NextNode, NavMesh.AllAreas, path);
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    unit.Paths.CurrentPath.Increment();
                    unit.Paths.PushPath(path.corners);
                }
            }
        }
    }
}

