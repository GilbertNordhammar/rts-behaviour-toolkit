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
        DebugSettings _debug;

        [SerializeField]
        protected BehaviourEntry[] _behaviours;

        // Public interface
        public virtual void CommandGoTo(List<RBTUnit> units, Vector3 destination)
        {
            var groups = GenerateGoToGroups(units, destination);
            _commandGroups.AddRange(groups);
        }

        public virtual void CommandPatrol(List<RBTUnit> units, Vector3 destination)
        {
            var groups = GeneratePatrolGroups(units, destination);
            _commandGroups.AddRange(groups);
        }

        public virtual void CommandAttack(List<RBTUnit> units, IAttackable target)
        {
            var groups = GenerateAttackGroups(units, target);
            _commandGroups.AddRange(groups);
        }
        public virtual void CommandFollow(List<RBTUnit> units, GameObject target)
        {
            var groups = GenerateFollowGroups(units, target);
            _commandGroups.AddRange(groups);
        }

        // Protected
        protected class CommandGroupList : List<CommandGroup>
        {
            public new void Add(CommandGroup item)
            {
                base.Add(item);
                NotifyBehaviours(item);
            }

            public new void AddRange(IEnumerable<CommandGroup> items)
            {
                base.AddRange(items);

                foreach (var item in items)
                    NotifyBehaviours(item);
            }

            public void Init(BehaviourEntry[] behaviourEntries)
            {
                _behaviourEntries = behaviourEntries;
            }

            void NotifyBehaviours(CommandGroup item)
            {
                foreach (var entry in _behaviourEntries)
                {
                    if (item.RemoveImmediately)
                        break;
                    else if (entry.enabled)
                        entry.behaviour.OnCommandGroupCreated(item);
                }
            }

            BehaviourEntry[] _behaviourEntries;
        }

        protected CommandGroupList _commandGroups = new CommandGroupList();
        protected UnitGrid _unitGrid = new UnitGrid();

        protected abstract List<GoToGroup> GenerateGoToGroups(List<RBTUnit> units, Vector3 destination);
        protected abstract List<PatrolGroup> GeneratePatrolGroups(List<RBTUnit> units, Vector3 destination);
        protected abstract List<AttackGroup> GenerateAttackGroups(List<RBTUnit> units, IAttackable target);
        protected abstract List<FollowGroup> GenerateFollowGroups(List<RBTUnit> units, GameObject target);


        [Serializable]
        protected class BehaviourEntry
        {
            public bool enabled = true;
            public UnitBehaviour behaviour;
        }

        protected enum PathsDisplayMode
        {
            None, All, MainPath, SubPaths
        }

        [Serializable]
        protected class DebugSettings
        {
            public bool showUnitGrid = false;
            public PathsDisplayMode pathsDisplay;
            public bool showBehaviours;
        }

        // Private
        void UpdateCommandGroups()
        {
            var groupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                commandGroup.Update();
                if (commandGroup.Units.Count == 0 || commandGroup.Remove)
                    groupsToRemove.Add(commandGroup);
            }

            RemoveCommandGroups(groupsToRemove);
        }

        void RemoveCommandGroups(List<CommandGroup> groups)
        {
            foreach (var group in groups)
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
                _unitGrid.Add(evnt.Sender);
            };

            RBTUnit.OnDeactivated += (evnt) =>
            {
                _unitGrid.Remove(evnt.Sender);
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

            var groupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                foreach (var behaviourEntry in _behaviours)
                {
                    if (commandGroup.RemoveImmediately)
                    {
                        groupsToRemove.Add(commandGroup);
                        break;
                    }
                    else if (behaviourEntry.enabled)
                        behaviourEntry.behaviour.OnUpdate(commandGroup);
                }
            }

            RemoveCommandGroups(groupsToRemove);
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
                if (_debug.showBehaviours)
                {
                    foreach (var behaviourEntry in _behaviours)
                    {
                        if (behaviourEntry.enabled)
                            behaviourEntry.behaviour.DrawGizmos(group);
                    }
                }

                if (_debug.pathsDisplay == PathsDisplayMode.None) continue;
                foreach (var commandUnit in group.Units)
                {
                    for (int i = 0; i < commandUnit.Paths.Count; i++)
                    {
                        if (i == 0 && _debug.pathsDisplay == PathsDisplayMode.SubPaths)
                            continue;
                        else if (i > 0 && _debug.pathsDisplay == PathsDisplayMode.MainPath)
                            break;

                        Gizmos.color = i == 0 ? Color.red : Color.blue;
                        var path = commandUnit.Paths[i];
                        for (int n = path.PreviousNodeIndex; n < path.Nodes.Length; n++)
                        {
                            Vector3 node1, node2;
                            if (n == path.PreviousNodeIndex)
                            {
                                node1 = commandUnit.Unit.Position;
                                node2 = path.Nodes.Length > 1 && (n + 1) < path.Nodes.Length ? path.Nodes[n + 1] : path.Nodes[n];
                                Gizmos.DrawSphere(node2, 0.2f);
                                Gizmos.DrawLine(node1, node2);
                            }
                            else
                            {
                                node1 = path.Nodes[n];
                                if ((n + 1) < path.Nodes.Length)
                                {
                                    node2 = path.Nodes[n + 1];
                                    Gizmos.DrawSphere(node2, 0.2f);
                                    Gizmos.DrawLine(node1, node2);
                                }
                            }
                            Gizmos.DrawSphere(node1, 0.2f);
                        }
                    }
                }
            }

            if (_debug.showUnitGrid)
                _unitGrid.DrawGizmos(UnitGrid.GizmosDrawMode.Wire, Color.white);

            Gizmos.color = originalColor;
        }
    }
}

