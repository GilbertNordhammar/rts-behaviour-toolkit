using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(NavMesh))]
    public abstract class UnitBehaviourManagerBase : MonoBehaviour
    {
        // Inspector
        [SerializeField]
        protected bool _showUnitGrid = false;

        [SerializeField]
        protected bool _showPaths = false;

        [SerializeField]
        protected bool _showBehaviours;

        [SerializeField]
        protected BehaviourEntry[] _behaviours;

        // Protected

        protected class CommandGroupList : List<CommandGroup>
        {
            public new void Add(CommandGroup item)
            {
                base.Add(item);
                foreach (var entry in _behaviourEntries)
                {
                    if (entry.enabled)
                        entry.behaviour.OnCommandGroupCreated(item);
                }
            }

            public void Init(BehaviourEntry[] behaviourEntries)
            {
                _behaviourEntries = behaviourEntries;
            }

            BehaviourEntry[] _behaviourEntries;
        }

        protected CommandGroupList _commandGroups = new CommandGroupList();
        protected UnitGrid _unitGrid = new UnitGrid();
        protected abstract List<List<CommandUnit>> CalcUnitsGroupsPerCommand(List<RBTUnit> commandedUnits, Vector3 destination);

        [System.Serializable]
        protected class BehaviourEntry
        {
            public bool enabled = true;
            public UnitBehaviour behaviour;
        }

        // Abstract interface
        public virtual void CommandGoTo(List<RBTUnit> units, Vector3 destination)
        {
            var unitGroups = CalcUnitsGroupsPerCommand(units, destination);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new GoToGroup(group, destination));
        }

        public virtual void CommandPatrol(List<RBTUnit> units, Vector3 destination)
        {
            var unitGroups = CalcUnitsGroupsPerCommand(units, destination);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new PatrolGroup(group, destination));
        }

        public virtual void CommandAttack(List<RBTUnit> units, IAttackable target)
        {
            var unitGroups = CalcUnitsGroupsPerCommand(units, target.Position);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new AttackGroup(group, target));
        }
        public virtual void CommandFollow(List<RBTUnit> units, GameObject target)
        {
            var unitGroups = CalcUnitsGroupsPerCommand(units, target.transform.position);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new FollowGroup(group, target));
        }

        // Private
        void UpdateCommandGroups()
        {
            var commandGroupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                commandGroup.Update();
                if (commandGroup.Units.Count == 0)
                    commandGroupsToRemove.Add(commandGroup);
            }

            foreach (var group in commandGroupsToRemove)
            {
                foreach (var unit in group.Units)
                    unit.Unit.ClearCommandGroup();
                _commandGroups.Remove(group);
            }
        }

        // Unity functions
        protected virtual void Awake()
        {
            RBTUnit.OnActivated += (evnt) =>
            {
                _unitGrid.Add(evnt.sender);
            };

            RBTUnit.OnDeactivated += (evnt) =>
            {
                _unitGrid.Remove(evnt.sender);
            };

            for (int i = 0; i < _behaviours.Length; i++)
                _behaviours[i].behaviour = Instantiate(_behaviours[i].behaviour);

            _commandGroups.Init(_behaviours);
        }

        protected virtual void Update()
        {
            // Gizmos flash like crazy if this is put in FixedUpdate(),
            // but performance improves
            foreach (var unit in RBTUnit.ActiveUnits)
                _unitGrid.Update(unit);
        }

        protected virtual void FixedUpdate()
        {
            UpdateCommandGroups();

            foreach (var commandGroup in _commandGroups)
            {
                foreach (var behaviourEntry in _behaviours)
                {
                    if (behaviourEntry.enabled)
                        behaviourEntry.behaviour.OnUpdate(commandGroup);
                }
            }
        }

        // Unity editor functions
        protected virtual void Start()
        {
            foreach (var unit in RBTUnit.ActiveUnits)
                _unitGrid.Add(unit);
        }

        protected virtual void OnDrawGizmos()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.red;

            foreach (var group in _commandGroups)
            {
                if (_showBehaviours)
                {
                    foreach (var behaviourentry in _behaviours)
                    {
                        if (behaviourentry.enabled)
                            behaviourentry.behaviour.DrawGizmos(group);
                    }
                }

                if (!_showPaths) continue;
                foreach (var commandUnit in group.Units)
                {
                    var pathNodes = commandUnit.PathQueue.Last().Nodes;
                    for (int i = 0; i < pathNodes.Length; i++)
                    {
                        var node1 = pathNodes[i];
                        Gizmos.DrawSphere(node1, 0.2f);
                        if ((i + 1) < pathNodes.Length)
                        {
                            var node2 = pathNodes[i + 1];
                            Gizmos.DrawSphere(node2, 0.2f);
                            Gizmos.DrawLine(node1, node2);
                        }
                    }
                }
            }

            if (_showUnitGrid)
                _unitGrid.DrawGizmos(UnitGrid.GizmosDrawMode.Wire, new Color(1, 0, 0, 0.5f));

            Gizmos.color = originalColor;
        }
    }
}

