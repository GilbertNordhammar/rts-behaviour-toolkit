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
        public override void OnCommandGroupCreated(CommandGroup group)
        {
            Vector3 destination = Vector3.zero;
            if (group is GoToGroup)
            {
                var goToGroup = (GoToGroup)group;
                destination = goToGroup.Destination;
            }
            else if (group is PatrolGroup)
            {
                throw new NotImplementedException();
            }
            else if (group is FollowGroup)
            {
                var followGroup = group as FollowGroup;
                destination = followGroup.Target.transform.position;
            }
            else // attack group
            {
                throw new NotImplementedException();
            }

            var commonData = new CommonGroupData();
            group.AddCustomData(commonData);

            group.OnUnitsWillBeRemoved += (evnt) =>
            {
                commonData.Reset();
            };
        }

        public override void OnUpdate(CommandGroup group)
        {
            var commonData = group.GetCustomData<CommonGroupData>();
            commonData.Update(group.Units, 0);
            var commander = commonData.Commander;

            var targetPos = Vector3.zero;
            var mayUpdatePaths = commonData.NewCommander;
            var targetIsUnit = false;
            if (group is GoToGroup)
            {
                var goToGroup = group as GoToGroup;
                targetPos = goToGroup.Destination;
            }
            else if (group is FollowGroup)
            {
                var followGroup = group as FollowGroup;
                targetPos = followGroup.Target.transform.position;
                var unit = followGroup.Target.GetComponent<RBTUnit>();
                mayUpdatePaths |= (unit && unit.Velocity != Vector3.zero);
                targetIsUnit = true;
            }

            if (mayUpdatePaths)
            {
                SetCommanderPath(commonData, targetPos, targetIsUnit);
                EnsureNextNodeIsReachable(commander);

                if (group.Units.Count > 0)
                    SetUnitPaths(commonData, group.Units);

                for (int i = 1; i < group.Units.Count; i++)
                {
                    EnsureNextNodeIsReachable(group.Units[i]);
                }
            }

            foreach (var unit in group.Units)
            {
                if (unit.Paths.CurrentPath)
                {
                    var prevNext = unit.Paths.CurrentPath.PreviousNextNodeIndex;
                    var next = unit.Paths.CurrentPath.NextNodeIndex;
                    if (prevNext != next)
                        EnsureNextNodeIsReachable(unit);
                }
            }
        }

        // Private
        void SetUnitPaths(CommonGroupData data, IReadOnlyList<CommandUnit> units)
        {
            var commanderNodes = data.Commander.Paths.CurrentPath.Nodes;
            for (int i = 0; i < units.Count; i++)
            {
                if (i == data.CommanderIndex) continue;

                var mag = data.OffsetsUnitToCenter[i].magnitude;
                var dir = (units[i].Unit.Position - data.Center).normalized;
                var posOffset = data.OffsetCommanderToCenter + dir * mag;
                var nodes = new Vector3[commanderNodes.Length];
                Array.Copy(commanderNodes, nodes, commanderNodes.Length);
                for (int j = 0; j < nodes.Length; j++)
                    nodes[j] += posOffset;
                units[i].Paths.ClearPaths();
                units[i].Paths.CurrentPath = new Path(nodes);
            }
        }

        void SetCommanderPath(CommonGroupData data, Vector3 targetPos, bool targetIsUnit)
        {
            var commander = data.Commander;
            Vector3 offset;
            // if (data.OffsetCommanderToCenter == Vector3.zero && targetIsUnit)
            //     offset = (targetPos - data.Commander.Unit.Position).normalized;
            // else
            //     offset = data.OffsetCommanderToCenter;
            offset = data.OffsetCommanderToCenter;
            var destination = targetPos - offset;

            var nodes = new Vector3[] { destination };
            commander.Paths.ClearPaths();
            commander.Paths.CurrentPath = new Path(nodes);
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

                    var nodes = new Vector3[path.corners.Length - 1];
                    Array.Copy(path.corners, 1, nodes, 0, nodes.Length);
                    unit.Paths.PushPath(nodes);
                }
            }
        }
    }
}

