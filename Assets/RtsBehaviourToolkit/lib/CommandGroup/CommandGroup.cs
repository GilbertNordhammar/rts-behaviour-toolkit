using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Assertions;

namespace RtsBehaviourToolkit
{
    public enum CommandType
    {
        Attack, Follow, Patrol, GoToAndStop
    }

    public class AttackGroup : CommandGroup
    {
        public AttackGroup(List<CommandUnit> units, int commanderIndex, IAttackable target)
            : base(units, commanderIndex)
        {
            Target = target;
        }

        public IAttackable Target { get; }
    }

    public class FollowGroup : CommandGroup
    {
        public FollowGroup(List<CommandUnit> units, int commanderIndex, GameObject target)
            : base(units, commanderIndex)
        {
            Target = target;
        }

        public GameObject Target { get; }
    }

    public class GoToGroup : CommandGroup
    {
        public GoToGroup(List<RBTUnit> units, Vector3 destination)
            : base(units)
        {
            Destination = destination;
        }

        public GoToGroup(List<CommandUnit> units, int commanderIndex, Vector3 destination)
            : base(units, commanderIndex)
        {
            Destination = destination;
        }

        public Vector3 Destination { get; }
    }

    public class PatrolGroup : CommandGroup
    {
        public PatrolGroup(List<CommandUnit> units, int commanderIndex, Vector3 destination)
            : base(units, commanderIndex)
        {
            Destination = destination;
        }

        public Vector3 Destination { get; }
    }

    [Serializable]
    public abstract partial class CommandGroup
    {
        // Public
        public CommandGroup(List<RBTUnit> units)
            : this(units.Select(unit => new CommandUnit(unit)).ToList(), 0)
        {
        }

        public CommandGroup(List<CommandUnit> units, int commanderIndex)
        {
            Assert.IsTrue(commanderIndex >= 0 && commanderIndex < units.Count, "Commander index must be within index bounds");
            Units = new List<CommandUnit>(units);

            Commander = Units[commanderIndex];

            var center = Vector3.zero;
            foreach (var unit in Units)
                center += unit.Unit.Position;
            center /= Units.Count;
            _commanderToCenterOffset = center - Commander.Unit.Position;

            foreach (var unit in Units)
                unit.Unit.AssignCommandGroup(Id);
        }

        public void Update()
        {
            var unitsToRemove = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                if (unit.Unit.CommandGroupId == Id && !unit.Remove)
                    unit.Update();
                else
                {
                    _onChangedGroup.Invoke(new UnitChangedGroupEvent(this, unit));
                    unitsToRemove.Add(unit);
                }
            }

            foreach (var unit in unitsToRemove)
                Units.Remove(unit);

            InvokePathEvents();
        }

        public void AddCustomData<T>(T data) where T : class
        {
            if (GetCustomData<T>(false) == null)
                _customDataObjects.Add(data);
            else
                Debug.LogWarning($"Custom data of type '{typeof(T).ToString()}' won't be added as it already exists on the command group");
        }

        public void RemoveCustomData<T>() where T : class
        {
            var data = GetCustomData<T>(false);
            if (data != null)
                _customDataObjects.Remove(data);
            else
                Debug.LogWarning($"Custom data of type '{typeof(T).ToString()}' can't be removed as it doesn't exist on the command group");
        }

        public T GetCustomData<T>(bool alertIfNoData = true) where T : class
        {
            T data = null;
            foreach (var dataObj in _customDataObjects)
            {
                data = dataObj as T;
                if (data != null)
                    break;
            }

            if (alertIfNoData)
                Debug.LogWarning($"Custom data of type '{typeof(T).ToString()}' can't be retrieved as it doesn't exist on the command group");

            return data;
        }

        public Vector3 Center { get => Commander.Unit.Position + _commanderToCenterOffset; }
        public CommandUnit Commander;
        public List<CommandUnit> Units { get; } = new List<CommandUnit>();
        public string Id { get; } = System.Guid.NewGuid().ToString();
        List<object> _customDataObjects = new List<object>();

        public static implicit operator bool(CommandGroup obj)
        {
            return obj != null;
        }

        // Private
        readonly Vector3 _commanderToCenterOffset;

        void InvokePathEvents()
        {
            foreach (var unit in Units)
            {
                var movementStatus = unit.Status;

                if (movementStatus.HasFlag(CommandUnit.PathStatus.NewPathNode))
                    _onNewPathNode.Invoke(new NewPathNodeEvent(this, unit));
                if (movementStatus.HasFlag(CommandUnit.PathStatus.NewPath))
                    _onNewPath.Invoke(new NewPathEvent(this, unit));
                if (movementStatus.HasFlag(CommandUnit.PathStatus.AllPathsTraversed))
                    _onMainPathTraversed.Invoke(new MainPathTraversedEvent(this, unit));
            }
        }
    }

    public class PathStack : IEnumerable<Path>
    {
        public void PushPath(Path item)
        {
            _paths.Add(item);
        }

        public void PushPath(Vector3[] pathNodes)
        {
            _paths.Add(new Path(pathNodes));
        }

        public void PopCurrentPath()
        {
            if (_paths.Count > 0)
                _paths.RemoveAt(_paths.Count - 1);
        }

        public Path CurrentPath
        {
            get => _paths.Count > 0 ? _paths.Last() : null;
            set
            {
                if (_paths.Count > 0)
                    _paths[_paths.Count - 1] = value;
                else
                    _paths.Add(value);
            }
        }

        public int Count { get => _paths.Count; }

        public int Capacity
        {
            get => _paths.Capacity;
            set => _paths.Capacity = value;
        }

        public void RemoveAt(int index)
        {
            _paths.RemoveAt(index);
        }

        public IEnumerator<Path> GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        public Path this[int index]
        {
            get => _paths[index];
            set => _paths[index] = value;
        }

        // private

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        List<Path> _paths = new List<Path>();
    }

    [Serializable]
    public class CommandUnit
    {
        // Public
        public CommandUnit(RBTUnit unit)
        {
            Unit = unit;
        }

        public CommandUnit(RBTUnit unit, Vector3[] pathNodes)
            : this(unit)
        {
        }

        public RBTUnit Unit { get; }
        public PathStack Paths { get; } = new PathStack();
        public PathStatus Status { get; private set; }
        public bool Remove { get; set; }

        public enum PathStatus
        {
            NoPaths = 1, TraversingPath = 2, NewPathNode = 4, NewPath = 8, AllPathsTraversed = 16
        }

        public void Update()
        {
            Status = PathStatus.NoPaths;
            if (Paths.Count == 0) return;

            Status = PathStatus.TraversingPath;
            var sqrOffset = Paths.CurrentPath.NextNode - Unit.Position;
            sqrOffset = Vector3.Scale(sqrOffset, sqrOffset);
            var samePosXZ = sqrOffset.x < 0.01f && sqrOffset.z < 0.01f;
            var sameAltitude = sqrOffset.y < 1.0; // TODO: Exchange 1.0 with unit height variable
            var sqrStepSize = Mathf.Pow(Unit.Speed * Time.fixedDeltaTime, 2); // Is this the actual step size?
            var sqrDistXZ = new Vector3(sqrOffset.x, 0, sqrOffset.z).sqrMagnitude;
            var reachedNextNode = samePosXZ && sameAltitude || sqrDistXZ < sqrStepSize;

            if (reachedNextNode)
            {
                Paths.CurrentPath.Increment();
                if (Paths.CurrentPath.Traversed)
                {
                    Paths.RemoveAt(Paths.Count - 1);
                    if (Paths.Count == 0)
                    {
                        Status |= PathStatus.AllPathsTraversed;
                        Status &= ~PathStatus.TraversingPath;
                    }
                    else Status |= PathStatus.NewPath;
                }

                if (Paths.Count > 0)
                    Status |= PathStatus.NewPathNode;
            }
        }

        public int NextCornerIndex
        {
            get => Paths.CurrentPath.NextNodeIndex;
        }

        public static implicit operator bool(CommandUnit obj)
        {
            return obj != null;
        }
    }
}
