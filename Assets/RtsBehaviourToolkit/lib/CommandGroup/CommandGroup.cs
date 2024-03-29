using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public AttackGroup(List<RBTUnit> units, IAttackable target)
            : base(units)
        {
            Target = target;
        }

        public AttackGroup(List<CommandUnit> units, IAttackable target)
            : base(units)
        {
            Target = target;
        }

        public IAttackable Target { get; }

        protected override void PreRemoveUnits(List<int> unitIndexes)
        {
            foreach (var i in unitIndexes)
            {
                if (_units[i].Unit.AttackTarget == Target)
                    _units[i].Unit.AttackTarget = null;
            }
        }
    }

    public class FollowGroup : CommandGroup
    {
        public FollowGroup(List<RBTUnit> units, GameObject target)
            : base(units)
        {
            Target = target;
        }

        public FollowGroup(List<CommandUnit> units, GameObject target)
            : base(units)
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

        public GoToGroup(List<CommandUnit> units, Vector3 destination)
            : base(units)
        {
            Destination = destination;
        }

        public Vector3 Destination { get; }
    }

    public class PatrolGroup : CommandGroup
    {
        public PatrolGroup(List<RBTUnit> units, Vector3 destination)
            : base(units)
        {
            Destination = destination;
        }

        public PatrolGroup(List<CommandUnit> units, Vector3 destination)
            : base(units)
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
            : this(units.Select(unit => new CommandUnit(unit)).ToList())
        {
        }

        public CommandGroup(List<CommandUnit> units)
        {
            _units = new List<CommandUnit>(units);

            foreach (var unit in _units)
                unit.Unit.AssignCommandGroup(Id);
        }

        public void Update()
        {
            var unitIndexesToRemove = new List<int>();

            for (int i = 0; i < _units.Count; i++)
            {
                if (_units[i].Unit.CommandGroupId != Id || _units[i].Remove)
                    unitIndexesToRemove.Add(i);
            }

            if (unitIndexesToRemove.Count > 0)
                _onUnitsWillBeRemoved.Invoke(new OnUnitsRemove(unitIndexesToRemove.ToArray()));
            PreRemoveUnits(unitIndexesToRemove);

            var nRemovedCounter = 0;
            foreach (var index in unitIndexesToRemove)
            {
                int i = index - nRemovedCounter;
                if (_units[i].Unit.CommandGroupId == Id)
                    _units[i].Unit.ClearCommandGroup();
                _units.RemoveAt(i);
                nRemovedCounter++;
            }
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

            if (data == null && alertIfNoData)
                Debug.LogWarning($"Custom data of type '{typeof(T).ToString()}' can't be retrieved as it doesn't exist on the command group");

            return data;
        }

        public IReadOnlyList<CommandUnit> Units { get => _units; }
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public bool Remove { get; set; }
        public bool RemoveImmediately { get; set; }

        public static implicit operator bool(CommandGroup me)
        {
            return !object.ReferenceEquals(me, null);
        }

        // Protected
        protected virtual void PreRemoveUnits(List<int> unitIndexes) { }
        protected List<CommandUnit> _units { get; } = new List<CommandUnit>();
        protected List<object> _customDataObjects = new List<object>();
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

        public void PopPath()
        {
            _paths.RemoveAt(_paths.Count - 1);
        }

        public void ClearPaths()
        {
            _paths.Clear();
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
        public bool Remove { get; set; }

        public int NextCornerIndex
        {
            get => Paths.CurrentPath.NextNodeIndex;
        }

        public static implicit operator bool(CommandUnit me)
        {
            return !object.ReferenceEquals(me, null);
        }
    }
}
