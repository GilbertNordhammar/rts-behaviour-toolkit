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
            var commonData = new CommonGroupData();
            group.AddCustomData(commonData);

            var rememberedPathStates = new RememberedPathStates(group.Units.Count);
            group.AddCustomData(rememberedPathStates);

            if (group is PatrolGroup)
            {
                var data = new PatrolData(group.Units);
                group.AddCustomData(data);
                group.OnUnitsWillBeRemoved += (evnt) =>
                {
                    data.RemoveRange(evnt.UnitsIndices);
                };
            }

            group.OnUnitsWillBeRemoved += (evnt) =>
            {
                rememberedPathStates.RemoveRange(evnt.UnitsIndices);
                commonData.Reset();
            };
        }

        public override void OnUpdate(CommandGroup group)
        {
            var commonData = group.GetCustomData<CommonGroupData>();
            commonData.Update(group.Units, 0);
            var commander = commonData.Commander;

            var targetPos = Vector3.zero;
            var mayUpdatePaths = commonData.NewCommander && !commander.Paths.CurrentPath;
            IMovable movable = null;
            PatrolData patrolData = null;
            PatrolGroup patrolGroup = null;
            if (group is GoToGroup)
            {
                var goToGroup = group as GoToGroup;
                targetPos = goToGroup.Destination;
            }
            else if (group is FollowGroup)
            {
                var followGroup = group as FollowGroup;
                targetPos = followGroup.Target.transform.position;
                movable = followGroup.Target.GetComponent<IMovable>();
            }
            else if (group is AttackGroup)
            {
                var attackGroup = group as AttackGroup;
                targetPos = attackGroup.Target.Position;
                movable = attackGroup.Target.GameObject.GetComponent<IMovable>();
            }
            else if (group is PatrolGroup)
            {
                patrolGroup = group as PatrolGroup;
                targetPos = patrolGroup.Destination;
                patrolData = patrolGroup.GetCustomData<PatrolData>(false);
                mayUpdatePaths = !patrolData.HasGeneratedPaths;
                if (!patrolData.HasGeneratedPaths)
                    patrolData.HasGeneratedPaths = true;
            }

            if (movable != null)
                mayUpdatePaths |= movable.Velocity != Vector3.zero;

            if (mayUpdatePaths)
            {
                SetCommanderPath(commonData, targetPos);
                EnsureNextNodeIsReachable(commander);

                if (group.Units.Count > 0)
                    SetUnitPaths(commonData, group.Units);
            }

            var rememberedPathStates = group.GetCustomData<RememberedPathStates>();
            for (int i = 0; i < group.Units.Count; i++)
            {
                var unit = group.Units[i];
                var currentPath = unit.Paths.CurrentPath;
                if (currentPath && !currentPath.Traversed)
                {
                    var lastPathState = rememberedPathStates.LastPathState[i];
                    if (currentPath.NextNodeIndex != lastPathState?.LastNextNodeIndex
                        || currentPath != lastPathState?.LastCurrentPath)
                    {
                        EnsureNextNodeIsReachable(unit);
                    }
                }
                else if (patrolGroup && !currentPath)
                {
                    var reversedPath = new Path(new Vector3[] { patrolData.SourcePosition[i] });
                    patrolData.SourcePosition[i] = unit.Unit.Position;
                    unit.Paths.PushPath(reversedPath);
                    EnsureNextNodeIsReachable(unit);
                }
            }

            rememberedPathStates.Update(group.Units);
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

        void SetCommanderPath(CommonGroupData data, Vector3 targetPos)
        {
            var commander = data.Commander;
            Vector3 offset = data.OffsetCommanderToCenter;
            var destination = targetPos - offset;
            var nodes = new Vector3[] { destination };
            commander.Paths.ClearPaths();
            commander.Paths.CurrentPath = new Path(nodes);
        }

        void EnsureNextNodeIsReachable(CommandUnit unit)
        {
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

    public class PatrolData
    {
        public PatrolData(IReadOnlyList<CommandUnit> units)
        {
            SourcePosition = units.Select(unit => unit.Unit.Position).ToList();
        }

        public void RemoveRange(int[] unitIndexes)
        {
            var nRemoved = 0;
            foreach (var index in unitIndexes)
            {
                SourcePosition.RemoveAt(index - nRemoved);
                nRemoved++;
            }
        }

        public List<Vector3> SourcePosition { get; } = new List<Vector3>();
        public bool HasGeneratedPaths = false;
    }

    public class RememberedPathStates
    {
        public RememberedPathStates(int nUnits)
        {
            LastPathState.Capacity = nUnits;
            for (int n = 0; n < nUnits; n++)
                LastPathState.Add(null);
        }

        public void Update(IReadOnlyList<CommandUnit> units)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Paths.CurrentPath)
                {
                    if (LastPathState[i] != null)
                        LastPathState[i].Update(units[i].Paths.CurrentPath);
                    else
                        LastPathState[i] = new PathState(units[i].Paths.CurrentPath);
                }
                else
                    LastPathState[i] = null;
            }
        }

        public void RemoveRange(int[] unitIndexes)
        {
            var nRemoved = 0;
            foreach (var index in unitIndexes)
            {
                LastPathState.RemoveAt(index - nRemoved);
                nRemoved++;
            }
        }

        public List<PathState> LastPathState { get; } = new List<PathState>();
    }

    public class PathState
    {
        public PathState(Path currentPath)
        {
            Update(currentPath);
        }

        public void Update(Path currentPath)
        {
            LastCurrentPath = currentPath;
            LastNextNodeIndex = currentPath.NextNodeIndex;
        }

        public int LastNextNodeIndex { get; private set; }
        public Path LastCurrentPath { get; private set; }
    }

}

