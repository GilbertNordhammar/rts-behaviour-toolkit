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
        protected List<CommandGroup> _commandGroups = new List<CommandGroup>();
        protected UnitGrid _unitGrid = new UnitGrid();
        protected abstract List<CommandGroup> GenerateCommandGroups(List<RBTUnit> units, Vector3 destination);

        [System.Serializable]
        protected class BehaviourEntry
        {
            public bool enabled = true;
            public UnitBehaviour behaviour;
        }

        // Abstract interface
        public abstract void CommandMovement(List<RBTUnit> units, Vector3 destination);
        public abstract void CommandPatrol(List<RBTUnit> units, Vector3 destination);
        public abstract void CommandAttack(List<RBTUnit> units, RBTUnit unit);
        public abstract void CommandFollow(List<RBTUnit> units, GameObject obj);

        // Private
        void HandleOnCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
        {
            var commandGroups = GenerateCommandGroups(evnt.Units, evnt.Position);
            foreach (var behaviourEntry in _behaviours)
            {
                if (!behaviourEntry.enabled) continue;
                foreach (var group in commandGroups)
                {
                    behaviourEntry.behaviour.OnCommandGroupCreated(group);
                }
            }
            _commandGroups.AddRange(commandGroups);
        }

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
            if (RBTUnitCommander.Instance)
                RBTUnitCommander.Instance.OnCommandGiven += HandleOnCommandGiven;
            else
                Debug.LogWarning($"Behaviour manager on {gameObject.name} couldn't subscribe to RBTUnitCommander.Instance.OnCommandGiven");

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

