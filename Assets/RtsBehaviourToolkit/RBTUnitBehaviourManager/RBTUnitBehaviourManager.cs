using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(NavMesh))]
    public partial class RBTUnitBehaviourManager : MonoBehaviour
    {
        // Unity editor

        // Public
        public static RBTUnitBehaviourManager Instance { get; private set; }

        // Private
        List<RBTUnitBehaviour> _unitBehaviours;
        List<CommandGroup> _commandGroups = new List<CommandGroup>();

        void HandleOnCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
        {
            var commandUnits = new List<CommandUnit>();
            foreach (var unit in evnt.Units)
            {
                var path = new NavMeshPath();
                NavMesh.CalculatePath(unit.transform.position, evnt.Position, NavMesh.AllAreas, path);
                commandUnits.Add(new CommandUnit(unit, path));
            }

            var randIndex = Random.Range(0, evnt.Units.Count);
            var leader = evnt.Units[randIndex];

            _commandGroups.Add(new CommandGroup(commandUnits, leader));
        }

        void UpdateCommandGroups()
        {
            var commandGroupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                var unitsToRemove = new List<CommandUnit>();
                foreach (var unit in commandGroup.CommandUnits)
                {
                    if (unit.HasTraversedPath || unit.Unit.CommandGroupId != commandGroup.Id)
                        unitsToRemove.Add(unit);
                }

                foreach (var unit in unitsToRemove)
                    commandGroup.CommandUnits.Remove(unit);

                if (commandGroup.CommandUnits.Count == 0)
                    commandGroupsToRemove.Add(commandGroup);
            }

            foreach (var group in commandGroupsToRemove)
                _commandGroups.Remove(group);
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitBehaviourManager on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;

            _unitBehaviours = GetComponentsInChildren<RBTUnitBehaviour>().ToList();
        }

        void FixedUpdate()
        {
            UpdateCommandGroups();

            foreach (var behaviour in _unitBehaviours)
            {
                foreach (var commandGroup in _commandGroups)
                    behaviour.Execute(commandGroup);
            }
        }

        // Unity editor functions
        void Start()
        {
            if (RBTUnitCommander.Instance)
                RBTUnitCommander.Instance.OnCommandGiven += HandleOnCommandGiven;
            else
                Debug.LogError("RBTUnitCommander couldn't subscribe to RBTUnitCommander.Instance.OnCommandGiven");
        }

        void OnDrawGizmos()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.red;

            if (_commandGroups.Count > 0)
            {
                foreach (var group in _commandGroups)
                {
                    foreach (var commandUnit in group.CommandUnits)
                    {
                        foreach (var corner in commandUnit.Path.corners)
                        {
                            Gizmos.DrawSphere(corner, 0.3f);
                        }
                    }
                }
            }

            Gizmos.color = originalColor;
        }
    }
}

