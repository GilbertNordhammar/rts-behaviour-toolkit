using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public enum CommandType
    {
        Attack, Follow, Patrol, GoToAndStop
    }

    public class AttackGroup : CommandGroup
    {
        public AttackGroup(List<CommandUnit> units, IAttackable target)
            : base(units)
        {
            Target = target;
        }

        public IAttackable Target { get; }
    }

    public class FollowGroup : CommandGroup
    {
        public FollowGroup(List<CommandUnit> units, GameObject target)
            : base(units)
        {
            Target = target;
        }

        public GameObject Target { get; }
    }

    public class GoToGroup : CommandGroup
    {
        public GoToGroup(List<CommandUnit> units, Vector3 destination)
            : base(units)
        {
            Destination = destination;
        }

        public override void Update()
        {
            base.Update();

            var unitsAtDestination = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                if (unit.Status.HasFlag(CommandUnit.MovementStatus.MainPathTraversed))
                    unitsAtDestination.Add(unit);
            }

            foreach (var unit in unitsAtDestination)
                Units.Remove(unit);
        }

        public Vector3 Destination { get; }
    }

    public class PatrolGroup : CommandGroup
    {
        public PatrolGroup(List<CommandUnit> units, Vector3 destination)
            : base(units)
        {
            Destination = destination;
        }

        public override void Update()
        {
            base.Update();

            foreach (var unit in Units)
            {
                if (unit.Status.HasFlag(CommandUnit.MovementStatus.MainPathTraversed))
                    unit.MainPath = new Path(unit.MainPath.Nodes.Reverse().ToArray());
            }
        }

        public Vector3 Destination { get; }
    }

    [Serializable]
    public abstract partial class CommandGroup
    {
        // Public
        public CommandGroup(List<CommandUnit> units)
        {
            Units = new List<CommandUnit>(units);

            foreach (var unit in Units)
                unit.Unit.AssignCommandGroup(Id);
        }

        public virtual void Update()
        {
            var unitsInNewGroup = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                if (unit.Unit.CommandGroupId == Id)
                    unit.Update();
                else
                {
                    _onChangedGroup.Invoke(new UnitChangedGroupEvent(this, unit));
                    unitsInNewGroup.Add(unit);
                }
            }

            foreach (var unit in unitsInNewGroup)
                Units.Remove(unit);

            InvokeMovementEvents();
        }

        public List<CommandUnit> Units { get; } = new List<CommandUnit>();
        public string Id { get; } = System.Guid.NewGuid().ToString();

        public static implicit operator bool(CommandGroup obj)
        {
            return obj != null;
        }

        // Private

        void InvokeMovementEvents()
        {
            foreach (var unit in Units)
            {
                var movementStatus = unit.Status;

                if (movementStatus.HasFlag(CommandUnit.MovementStatus.NewPathNode))
                    _onNewPathNode.Invoke(new NewPathNodeEvent(this, unit));
                if (movementStatus.HasFlag(CommandUnit.MovementStatus.NewPath))
                    _onNewPath.Invoke(new NewPathEvent(this, unit));
                if (movementStatus.HasFlag(CommandUnit.MovementStatus.MainPathTraversed))
                    _onMainPathTraversed.Invoke(new MainPathTraversedEvent(this, unit));
            }
        }
    }

    [Serializable]
    public class CommandUnit
    {
        // Public
        public CommandUnit(RBTUnit unit, Path mainPath)
        {
            Unit = unit;
            PathQueue.Add(mainPath);
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

        public Path MainPath
        {
            get => PathQueue[0];
            set => PathQueue[0] = value;
        }

        public MovementStatus Status { get; private set; }

        public void PushSubPath(Path path)
        {
            PathQueue.Add(path);
        }

        public void PushPath(Vector3[] nodes)
        {
            PathQueue.Add(new Path(nodes));
        }

        public enum MovementStatus
        {
            TraversingPath = 1, NewPathNode = 2, NewPath = 4, MainPathTraversed = 8
        }

        public void Update()
        {
            Status = MovementStatus.TraversingPath;
            var absOffset = CurrentPath.NextCorner - Unit.transform.position;
            absOffset = new Vector3(Mathf.Abs(absOffset.x), Mathf.Abs(absOffset.y), Mathf.Abs(absOffset.z));
            var reachedNextNode = absOffset.x < 0.1 && absOffset.z < 0.1 && absOffset.y < 1.0; // TODO: base "absOffset.y < 1.0" off of unit height
            if (reachedNextNode)
            {
                Status |= MovementStatus.NewPathNode;

                CurrentPath.Increment();
                if (CurrentPath.Traversed)
                {
                    if (PathQueue.Count == 1)
                    {
                        Status |= MovementStatus.MainPathTraversed;
                        Status &= ~MovementStatus.TraversingPath;
                    }
                    else
                    {
                        PathQueue.RemoveAt(PathQueue.Count - 1);
                        Status |= MovementStatus.NewPath;
                    }
                }
            }
        }

        public int NextCornerIndex
        {
            get => CurrentPath.NextCornerIndex;
        }

        public static implicit operator bool(CommandUnit obj)
        {
            return obj != null;
        }
    }
}
