using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public enum CommandType
    {
        Attack, Follow, Patrol, GoToAndStop
    }

    [Serializable]
    public partial class CommandGroup
    {
        public CommandGroup(List<CommandUnit> units, Vector3 destination, CommandType commandType)
        {
            Units = new List<CommandUnit>(units);
            Destination = destination;
            Command = commandType;

            foreach (var unit in Units)
            {
                unit.Unit.AssignCommandGroup(Id);
            }
        }

        public void Update()
        {
            var finishedUnits = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                var prevPathNode = unit.CurrentPath.NextCorner;
                var status = unit.Update();

                if (status.HasFlag(CommandUnit.MovementStatus.NewPathNode))
                {
                    _onNewPathNode.Invoke(new NewPathNodeEvent(this, unit, prevPathNode));
                }
                if (status.HasFlag(CommandUnit.MovementStatus.NewPath))
                    _onPathTraversed.Invoke(new PathTraversedEvent(this, unit, false));
                if (status.HasFlag(CommandUnit.MovementStatus.LastPathTraversed))
                {
                    _onPathTraversed.Invoke(new PathTraversedEvent(this, unit, true));
                    if (Command == CommandType.Patrol)
                        unit.CurrentPath = new Path(unit.CurrentPath.Nodes.Reverse().ToArray());
                    else
                        finishedUnits.Add(unit);
                }

                if (unit.Unit.CommandGroupId != Id)
                {
                    _onNewGroup.Invoke(new NewGroupEvent(this, unit));
                    finishedUnits.Add(unit);
                }
            }

            foreach (var unit in finishedUnits)
            {
                Units.Remove(unit);
            }
        }

        public List<CommandUnit> Units { get; } = new List<CommandUnit>();
        public Vector3 Destination { get; }
        public CommandType Command { get; }
        public string Id { get; } = System.Guid.NewGuid().ToString();
    }

    [Serializable]
    public class CommandUnit
    {
        // Public
        public CommandUnit(RBTUnit unit, Path path)
        {
            Unit = unit;
            PathQueue.Add(path);
        }

        public CommandUnit(RBTUnit unit, Vector3[] pathNodes)
            : this(unit, new Path(pathNodes))
        {
        }

        public RBTUnit Unit { get; }

        public List<Path> PathQueue { get; } = new List<Path>();
        public Path CurrentPath
        {
            get => PathQueue.Last();
            set => PathQueue[PathQueue.Count - 1] = value;
        }

        public void PushPath(Path path)
        {
            PathQueue.Add(path);
        }

        public void PushPath(Vector3[] nodes)
        {
            PathQueue.Add(new Path(nodes));
        }

        public enum MovementStatus
        {
            None = 0, NewPathNode = 1, NewPath = 2, LastPathTraversed = 4
        }

        public MovementStatus Update()
        {
            var status = MovementStatus.None;
            var absOffset = CurrentPath.NextCorner - Unit.transform.position;
            absOffset = new Vector3(Mathf.Abs(absOffset.x), Mathf.Abs(absOffset.y), Mathf.Abs(absOffset.z));
            if (absOffset.x < 0.1 && absOffset.z < 0.1 && absOffset.y < 1.0) // base "absOffset.y < 1.0" off of unit height
            {
                CurrentPath.Increment();
                if (CurrentPath.Traversed)
                {
                    if (PathQueue.Count == 1)
                        status |= MovementStatus.LastPathTraversed;
                    else
                    {
                        PathQueue.RemoveAt(PathQueue.Count - 1);
                        status |= MovementStatus.NewPath | MovementStatus.NewPathNode;
                    }
                }
                else status |= MovementStatus.NewPathNode;
            }

            return status;
        }

        public int NextCornerIndex
        {
            get => CurrentPath.NextCornerIndex;
        }
    }
}
