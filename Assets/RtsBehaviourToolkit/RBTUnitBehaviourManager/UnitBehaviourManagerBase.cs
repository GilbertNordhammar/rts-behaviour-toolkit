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
        protected PathsDisplayMode _pathsDisplay;

        [SerializeField]
        protected bool _showBehaviours;

        [SerializeField]
        protected BehaviourEntry[] _behaviours;

        // Protected
        protected enum PathsDisplayMode
        {
            None, All, MainPath, SubPaths
        }

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
        protected abstract List<List<CommandUnit>> CalcUnitGroupsPerCommand(List<RBTUnit> commandedUnits, Vector3 destination);

        [System.Serializable]
        protected class BehaviourEntry
        {
            public bool enabled = true;
            public UnitBehaviour behaviour;
        }

        // Abstract interface
        public virtual void CommandGoTo(List<RBTUnit> units, Vector3 destination)
        {
            var unitGroups = CalcUnitGroupsPerCommand(units, destination);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new GoToGroup(group, destination));
        }

        public virtual void CommandPatrol(List<RBTUnit> units, Vector3 destination)
        {
            var unitGroups = CalcUnitGroupsPerCommand(units, destination);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new PatrolGroup(group, destination));
        }

        public virtual void CommandAttack(List<RBTUnit> units, IAttackable target)
        {
            var unitGroups = CalcUnitGroupsPerCommand(units, target.Position);
            _commandGroups.Capacity += unitGroups.Count;
            foreach (var group in unitGroups)
                _commandGroups.Add(new AttackGroup(group, target));
        }
        public virtual void CommandFollow(List<RBTUnit> units, GameObject target)
        {
            var unitGroups = CalcUnitGroupsPerCommand(units, target.transform.position);
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

                if (_pathsDisplay == PathsDisplayMode.None) continue;
                foreach (var commandUnit in group.Units)
                {
                    Gizmos.color = Color.red;
                    var pLower = 0;
                    var pUpper = commandUnit.PathQueue.Count;
                    if (_pathsDisplay == PathsDisplayMode.MainPath)
                        pUpper = 1;
                    else if (_pathsDisplay == PathsDisplayMode.SubPaths)
                        pLower = 1;
                    for (int p = pLower; p < pUpper; p++)
                    {
                        var path = commandUnit.PathQueue[p];
                        for (int n = path.PreviousNodeIndex; n < path.Nodes.Length; n++)
                        {
                            var node1 = path.Nodes[n];
                            Gizmos.DrawSphere(node1, 0.2f);
                            if ((n + 1) < path.Nodes.Length)
                            {
                                var node2 = path.Nodes[n + 1];
                                Gizmos.DrawSphere(node2, 0.2f);
                                Gizmos.DrawLine(node1, node2);
                            }
                        }
                        Gizmos.color = Color.blue;
                    }
                }
            }


            if (_showUnitGrid)
            {
                Gizmos.color = Color.red;
                _unitGrid.DrawGizmos(UnitGrid.GizmosDrawMode.Wire, new Color(1, 0, 0, 0.5f));
            }

            Gizmos.color = originalColor;
        }
    }
}

